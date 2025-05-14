import React, { createContext, useContext, useState, useEffect, useMemo } from 'react';
import PropTypes from 'prop-types';
import AsyncStorageUtils from './AsyncStorage';
import ApiService from '../services/ApiService';
import Helpers from './Helpers';
import { router } from 'expo-router';
import Constants from './Constants';
import * as Localization from 'expo-localization';


const DEFAULT_CONSTANTS = {
  ROUTES: {
    LOGIN: 'login',
    REGISTER: 'register',
    WELCOME: '/welcome',
    MAIN: '/(drawer)/(tabs)',
    TEXT_VOICE: '/(drawer)/(tabs)/text-voice',
  },
  DEFAULT_PREFERENCES: {
    DEFAULT_FROM_LANG: 'en',
    DEFAULT_TO_LANG: 'he',
  },
  SESSION: {
    VALIDATION_INTERVAL: 15000, // Default to 15 seconds
  },
};

// Ensure SESSION is always defined
const ROUTES = Constants?.ROUTES || DEFAULT_CONSTANTS.ROUTES;
const DEFAULT_PREFERENCES = Constants?.DEFAULT_PREFERENCES || DEFAULT_CONSTANTS.DEFAULT_PREFERENCES;
const SESSION = Constants?.SESSION || DEFAULT_CONSTANTS.SESSION;

const SessionContext = createContext({
  session: null,
  preferences: {
    defaultFromLang: null,
    defaultToLang: null,
  },
  isLoading: true,
  isAuthLoading: false,
  error: null,
  signIn: async () => {},
  signOut: async () => {},
  resetSession: async () => {},
  resetSessionButKeepPreferences: async () => {},
  register: async () => {},
  setPreferences: async () => {},
  clearError: () => {},
  isLoggingIn: false,
});

export const SessionProvider = ({ children }) => {
  const [session, setSession] = useState(null);
  const [preferences, setPreferencesState] = useState({
    defaultFromLang: null,
    defaultToLang: null,
  });
  const [isLoading, setIsLoading] = useState(true);
  const [isAuthLoading, setIsAuthLoading] = useState(false);
  const [error, setError] = useState(null);
  const [lastLoginTime, setLastLoginTime] = useState(null);
  const [isLoggingIn, setIsLoggingIn] = useState(false);

  const clearError = () => {
    setError(null);
  };

  useEffect(() => {
    const initialize = async () => {
      try {
        setIsLoading(true);
        setError(null);

        const userData = await AsyncStorageUtils.getItem('user');
        const token = await AsyncStorageUtils.getItem('signed_session_id');
        let preferencesData = null;

        if (userData && userData.id && token) {
          const response = await ApiService.get('/validate-session', token);
          if (response.success) {
            setSession({ ...userData, signed_session_id: token });
            preferencesData = await AsyncStorageUtils.getItem('preferences');
          } else {
            await resetSession();
            preferencesData = await AsyncStorageUtils.getItem('preferences');
          }
        } else {
          await resetSessionButKeepPreferences();
          preferencesData = await AsyncStorageUtils.getItem('preferences');
        }

        if (preferencesData) setPreferencesState(preferencesData);
      } catch (err) {
        setError(Helpers.handleError(err));
        await resetSessionButKeepPreferences();
      } finally {
        setIsLoading(false);
      }
    };
    initialize();
  }, []);

  useEffect(() => {
    const validateSession = async () => {
      try {
        const token = await AsyncStorageUtils.getItem('signed_session_id');
        if (!token) {
          return;
        }

        const response = await ApiService.get('/validate-session', token);
        console.log('Session validation response:', response);

        if (!response.success) {
          const now = Date.now();
          if (lastLoginTime && now - lastLoginTime < 30000) {
            console.warn('Session validation failed shortly after login, ignoring...');
            return;
          }
          if (isLoggingIn) {
            console.warn('Session validation failed during login attempt, skipping navigation...');
            await resetSession();
            return;
          }
          console.warn('Session expired or invalid. Resetting...');
          await resetSession();
          router.replace(ROUTES.WELCOME);
        }
      } catch (err) {
        console.error('Error during session validation:', err);
      }
    };

    const timer = setTimeout(() => {
      validateSession();
      const interval = setInterval(validateSession, SESSION.VALIDATION_INTERVAL);
      return () => clearInterval(interval);
    }, 5000);

    return () => clearTimeout(timer);
  }, [lastLoginTime, isLoggingIn]);

  const resetSession = async () => {
    await AsyncStorageUtils.removeItem('user');
    await AsyncStorageUtils.removeItem('signed_session_id');
    await AsyncStorageUtils.removeItem('preferences');

    setSession(null);
    setPreferencesState({ defaultFromLang: null, defaultToLang: null });
  };

  const resetSessionButKeepPreferences = async () => {
    await AsyncStorageUtils.removeItem('signed_session_id');
    setSession(null);
  };

  const signIn = async (email, password) => {
    try {
      setIsLoggingIn(true);
      setIsAuthLoading(true);
      setError(null);

      const response = await ApiService.post('/login', { email, password });

      if (!response.success || !response.data || !response.data.token || !response.data.user) {
        throw new Error(response?.error || 'Login failed');
      }

      const { user, token } = response.data;

      await AsyncStorageUtils.setItem('user', user);
      await AsyncStorageUtils.setItem('signed_session_id', token);

      setSession({ ...user, signed_session_id: token });
      setPreferencesState({
        defaultFromLang: user.defaultFromLang || null,
        defaultToLang: user.defaultToLang || null,
      });

      setLastLoginTime(Date.now());
      router.replace(ROUTES.MAIN);
    } catch (err) {
      await resetSessionButKeepPreferences();
      const msg = Helpers.handleError(err);
      setError(msg);
      throw new Error(msg);
    } finally {
      setIsAuthLoading(false);
      setIsLoggingIn(false);
    }
  };

  const signOut = async () => {
    try {
      setIsAuthLoading(true);
      setError(null);

      const token = await AsyncStorageUtils.getItem('signed_session_id');
      const response = await ApiService.post('/logout', {}, token);
      if (!response.success) throw new Error(response.error || 'Logout failed');

      await resetSession();
      router.replace(ROUTES.WELCOME);
    } catch (err) {
      const msg = Helpers.handleError(err);
      setError(msg);
      throw new Error(msg);
    } finally {
      setIsAuthLoading(false);
    }
  };

  const register = async (email, password) => {
    try {
      setIsAuthLoading(true);
      setError(null);

      const response = await ApiService.post('/register', { email, password });
      if (!response.success) throw new Error(response.error || 'Registration failed');

      const deviceLocale = Localization.locale.split('-')[0]; // e.g., 'en-US' -> 'en'
      const defaultPrefs = {
        defaultFromLang: deviceLocale || DEFAULT_PREFERENCES.DEFAULT_FROM_LANG || 'en',
        defaultToLang: deviceLocale === 'he' ? 'en' : DEFAULT_PREFERENCES.DEFAULT_TO_LANG || 'he',
      };
      await AsyncStorageUtils.setItem('preferences', defaultPrefs);
      setPreferencesState(defaultPrefs);
    } catch (err) {
      const msg = Helpers.handleError(err);
      setError(msg);
      throw new Error(msg);
    } finally {
      setIsAuthLoading(false);
    }
  };

  const setPreferences = async (prefs) => {
    try {
      setIsAuthLoading(true);
      setError(null);

      const token = await AsyncStorageUtils.getItem('signed_session_id');
      const user = await AsyncStorageUtils.getItem('user');

      if (token && user?.id) {
        const response = await ApiService.post('/preferences', prefs, token);
        if (!response.success) throw new Error(response.error || 'Failed to update preferences');
      }

      await AsyncStorageUtils.setItem('preferences', prefs);
      setPreferencesState(prefs);
    } catch (err) {
      const msg = Helpers.handleError(err);
      setError(msg);
      throw new Error(msg);
    } finally {
      setIsAuthLoading(false);
    }
  };

  const contextValue = useMemo(() => ({
    session,
    preferences,
    isLoading,
    isAuthLoading,
    error,
    signIn,
    signOut,
    resetSession,
    resetSessionButKeepPreferences,
    register,
    setPreferences,
    clearError,
    isLoggingIn,
  }), [session, preferences, isLoading, isAuthLoading, error, isLoggingIn]);

  return (
    <SessionContext.Provider value={contextValue}>
      {children}
    </SessionContext.Provider>
  );
};

export const useSession = () => {
  const context = useContext(SessionContext);
  if (!context) {
    throw new Error('useSession must be used within a SessionProvider');
  }
  return context;
};

SessionProvider.propTypes = {
  children: PropTypes.node.isRequired,
};