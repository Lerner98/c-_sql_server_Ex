import React, { createContext, useContext, useState, useEffect, useMemo, useRef } from 'react';
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
};

const ROUTES = Constants?.ROUTES || DEFAULT_CONSTANTS.ROUTES;
const DEFAULT_PREFERENCES = Constants?.DEFAULT_PREFERENCES || DEFAULT_CONSTANTS.DEFAULT_PREFERENCES;

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
  const [hasLoggedIn, setHasLoggedIn] = useState(false);
  const [pendingNavigation, setPendingNavigation] = useState(null);

  const clearError = () => {
    setError(null);
  };

  useEffect(() => {
    if (pendingNavigation && !isLoading && !isAuthLoading) {
      console.log('[SessionProvider] Executing pending navigation to:', pendingNavigation);
      router.replace(pendingNavigation);
      setPendingNavigation(null);
    }
  }, [pendingNavigation, isLoading, isAuthLoading]);

  useEffect(() => {
    const initialize = async () => {
      try {
        console.log('[SessionProvider] 🚀 Starting initialization...');
        setIsLoading(true);
        setError(null);

        const userData = await AsyncStorageUtils.getItem('user');
        const token = await AsyncStorageUtils.getItem('signed_session_id');
        let preferencesData = await AsyncStorageUtils.getItem('preferences');

        console.log('[SessionProvider] 📊 Stored data check:', {
          hasUserData: !!userData,
          hasUserId: !!userData?.id,
          hasToken: !!token,
          tokenLength: token?.length,
          hasLoggedIn: hasLoggedIn,
        });

        // ✅ SIMPLIFIED: If hasLoggedIn is true and token exists, restore session without validation
        if (hasLoggedIn && userData?.id && token) {
          console.log('[SessionProvider] ✅ Restoring session for logged-in user');
          setSession({ ...userData, signed_session_id: token });
          setPendingNavigation(ROUTES.MAIN);
        }
        // Only validate session if not previously logged in
        else if (userData?.id && token && !hasLoggedIn) {
          console.log('[SessionProvider] 🔍 Found stored session, validating...');
          
          try {
            console.log('[SessionProvider] 📡 Calling validation endpoint...');
            const response = await ApiService.get('/api/auth/validate-session', token);
            
            console.log('[SessionProvider] 📨 Validation response:', {
              success: response.success,
              hasData: !!response.data,
              error: response.error
            });
            
            if (response.success) {
              console.log('[SessionProvider] ✅ Session is valid, restoring user');
              setSession({ ...userData, signed_session_id: token });
              setHasLoggedIn(true);
              setPendingNavigation(ROUTES.MAIN);
            } else {
              console.log('[SessionProvider] ❌ Session expired, clearing data');
              await resetSession();
            }
          } catch (validationError) {
            console.log('[SessionProvider] 💥 Session validation threw error:', validationError.message);
            await resetSession();
          }
        } else {
          console.log('[SessionProvider] ℹ️ No session data found');
          await resetSessionButKeepPreferences();
        }

        if (preferencesData) {
          console.log('[SessionProvider] 🎯 Loading preferences');
          setPreferencesState(preferencesData);
        }

      } catch (err) {
        console.error('[SessionProvider] 💥 Initialization error:', err);
        setError(Helpers.handleError(err));
        await resetSessionButKeepPreferences();
      } finally {
        console.log('[SessionProvider] 🏁 Initialization complete');
        setIsLoading(false);
      }
    };

    const timeoutId = setTimeout(initialize, 100);
    return () => clearTimeout(timeoutId);
  }, [hasLoggedIn]);

  const resetSession = async () => {
    console.log('[SessionProvider] 🗑️ Resetting complete session');
    await AsyncStorageUtils.removeItem('user');
    await AsyncStorageUtils.removeItem('signed_session_id');
    await AsyncStorageUtils.removeItem('preferences');
    setSession(null);
    setPreferencesState({ defaultFromLang: null, defaultToLang: null });
    setHasLoggedIn(false);
  };

  const resetSessionButKeepPreferences = async () => {
    console.log('[SessionProvider] 🗑️ Resetting session but keeping preferences');
    const token = await AsyncStorageUtils.getItem('signed_session_id');
    const userData = await AsyncStorageUtils.getItem('user');
    
    if (token && userData?.id && session) {
      console.log('[SessionProvider] 🔒 Active session detected, preserving session state');
      return;
    }

    await AsyncStorageUtils.removeItem('user');
    await AsyncStorageUtils.removeItem('signed_session_id');
    setSession(null);
  };

  const signIn = async (email, password) => {
    try {
      setIsAuthLoading(true);
      setError(null);

      console.log('[SessionProvider] 🔐 Signing in user:', email);
      const response = await ApiService.post('/api/auth/login', { email, password });

      console.log('[SessionProvider] 📨 Login response:', {
        success: response.success,
        hasData: !!response.data,
        hasToken: !!response.data?.token,
        hasUser: !!response.data?.user,
        error: response.error
      });

      if (!response.success || !response.data?.token || !response.data?.user) {
        throw new Error(response?.error || 'Login failed');
      }

      const { user, token } = response.data;

      console.log('[SessionProvider] 💾 Storing session data...');
      await AsyncStorageUtils.setItem('user', user);
      await AsyncStorageUtils.setItem('signed_session_id', token);

      console.log('[SessionProvider] 🎯 Updating state...');
      setSession({ ...user, signed_session_id: token });

      // ✅ Reset preferences for logged-in user
      const deviceLocale = Localization.locale.split('-')[0];
      const defaultPrefs = {
        defaultFromLang: user.defaultFromLang || deviceLocale || DEFAULT_PREFERENCES.DEFAULT_FROM_LANG || 'en',
        defaultToLang: user.defaultToLang || (deviceLocale === 'he' ? 'en' : DEFAULT_PREFERENCES.DEFAULT_TO_LANG || 'he'),
      };
      await AsyncStorageUtils.setItem('preferences', defaultPrefs);
      setPreferencesState(defaultPrefs);

      console.log('[SessionProvider] ✅ Sign in successful, navigating...');
      setHasLoggedIn(true);
      setPendingNavigation(ROUTES.MAIN);

    } catch (err) {
      console.error('[SessionProvider] 💥 Sign in failed:', err);
      await resetSessionButKeepPreferences();
      const msg = Helpers.handleError(err);
      setError(msg);
      throw new Error(msg);
    } finally {
      setIsAuthLoading(false);
    }
  };

  const signOut = async () => {
    try {
      setIsAuthLoading(true);
      setError(null);

      console.log('[SessionProvider] 🚪 Signing out user');
      const token = await AsyncStorageUtils.getItem('signed_session_id');
      
      if (token) {
        try {
          await ApiService.post('/api/auth/logout', {}, token);
          console.log('[SessionProvider] 📡 Server logout successful');
        } catch (logoutError) {
          console.warn('[SessionProvider] ⚠️ Server logout failed, continuing with local cleanup');
        }
      }

      await resetSession();
      router.replace(ROUTES.WELCOME);
      console.log('[SessionProvider] ✅ Sign out successful');

    } catch (err) {
      console.error('[SessionProvider] 💥 Sign out error:', err);
      const msg = Helpers.handleError(err);
      setError(msg);
      await resetSession();
      router.replace(ROUTES.WELCOME);
    } finally {
      setIsAuthLoading(false);
    }
  };

  const register = async (email, password) => {
    try {
      setIsAuthLoading(true);
      setError(null);

      console.log('[SessionProvider] 📝 Registering user:', email);
      const response = await ApiService.post('/api/auth/register', { email, password });
      
      if (!response.success) {
        throw new Error(response.error || 'Registration failed');
      }

      const deviceLocale = Localization.locale.split('-')[0];
      const defaultPrefs = {
        defaultFromLang: deviceLocale || DEFAULT_PREFERENCES.DEFAULT_FROM_LANG || 'en',
        defaultToLang: deviceLocale === 'he' ? 'en' : DEFAULT_PREFERENCES.DEFAULT_TO_LANG || 'he',
      };
      
      await AsyncStorageUtils.setItem('preferences', defaultPrefs);
      setPreferencesState(defaultPrefs);
      console.log('[SessionProvider] ✅ Registration successful');

    } catch (err) {
      console.error('[SessionProvider] 💥 Registration failed:', err);
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
        if (!response.success) {
          throw new Error(response.error || 'Failed to update preferences');
        }
      }

      await AsyncStorageUtils.setItem('preferences', prefs);
      setPreferencesState(prefs);

    } catch (err) {
      console.error('[SessionProvider] 💥 Set preferences failed:', err);
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
  }), [session, preferences, isLoading, isAuthLoading, error]);

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