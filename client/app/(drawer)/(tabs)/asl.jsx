import React, { useState, useRef, useEffect } from 'react';
import { View, Text, StyleSheet, ActivityIndicator, Alert, Pressable } from 'react-native';
import { CameraView, useCameraPermissions } from 'expo-camera';
import { useTranslation } from '../../../utils/TranslationContext';
import { useSession } from '../../../utils/ctx';
import useTranslationStore from '../../../stores/TranslationStore';
import TranslationService from '../../../services/TranslationService';
import Toast from '../../../components/Toast';
import Constants from '../../../utils/Constants';
import Helpers from '../../../utils/Helpers';
import useThemeStore from '../../../stores/ThemeStore';
import { useRouter } from 'expo-router';

// ✅ Safe conversion of any error to string to prevent (NOBRIDGE)
const getSafeMessage = (msg) => {
  if (typeof msg === 'string') return msg;
  if (msg instanceof Error && msg.message) return msg.message;
  if (React.isValidElement(msg)) return '[Invalid React Element]';
  try {
    return JSON.stringify(msg);
  } catch {
    return '[Unrenderable error object]';
  }
};

const { GUEST_TRANSLATION_LIMIT, COLORS, FONT_SIZES, SPACING } = Constants;

const ASLTranslationScreen = () => {
  const { t } = useTranslation();
  const { session } = useSession();
  const { addTextTranslation, incrementGuestTranslationCount, getGuestTranslationCount } = useTranslationStore();
  const { isDarkMode } = useThemeStore();
  const [hasPermission, setHasPermission] = useState(null);
  const [translatedText, setTranslatedText] = useState('');
  const [error, setError] = useState('');
  const [toastVisible, setToastVisible] = useState(false);
  const [isCameraActive, setIsCameraActive] = useState(false);
  const cameraRef = useRef(null);
  const translationIntervalRef = useRef(null);
  const [permission, requestPermission] = useCameraPermissions();
  const router = useRouter();

  useEffect(() => {
    const checkCameraPermission = async () => {
      if (!permission) return;

      if (!permission.granted) {
        const { status } = await requestPermission();
        if (status === 'granted') {
          setHasPermission(true);
        } else {
          setHasPermission(false);
          setError(t('error') + ': Camera permission not granted. Please enable it in your device settings.');
          setToastVisible(true);
        }
      } else {
        setHasPermission(true);
      }
    };

    checkCameraPermission();

    return () => {
      stopCamera();
      setTranslatedText('');
    };
  }, [permission, requestPermission, t]);

  const startCamera = async () => {
    setError('');
    if (hasPermission !== true) {
      setError(t('error') + ': Camera permission not granted. Please enable it in your device settings.');
      setToastVisible(true);
      return;
    }

    if (!session) {
      const aslCount = await getGuestTranslationCount('asl');
      if (aslCount >= GUEST_TRANSLATION_LIMIT) {
        setError(t('guestLimit'));
        setToastVisible(true);
        return;
      }
    }

    setIsCameraActive(true);
    setError('');

    translationIntervalRef.current = setInterval(async () => {
      try {
        const camera = cameraRef.current;
        if (camera) {
          const photo = await camera.takePictureAsync();
          const imageBase64 = await Helpers.fileToBase64(photo.uri);

          const recognizedText = await TranslationService.recognizeASL(imageBase64, session?.signed_session_id);
          if (!recognizedText) {
            setTranslatedText('');
            return;
          }

          setTranslatedText(recognizedText);

          await addTextTranslation({
            id: Date.now().toString(),
            fromLang: 'en',
            toLang: 'en',
            original_text: recognizedText,
            translated_text: recognizedText,
            created_at: new Date().toISOString(),
            type: 'asl',
          }, !session, session?.signed_session_id);

          if (!session) await incrementGuestTranslationCount('asl');
        } else {
          setError(t('error') + ': Camera not available');
          setToastVisible(true);
        }
      } catch (err) {
        console.error('ASL translation error:', err);
        setError(t('error') + ': ' + Helpers.handleError(err));
        setToastVisible(true);
      }
    }, 5000);
  };

  const stopCamera = () => {
    setIsCameraActive(false);
    if (translationIntervalRef.current) {
      clearInterval(translationIntervalRef.current);
      translationIntervalRef.current = null;
    }
  };

  const handleDeleteTranslation = () => {
    setTranslatedText('');
  };

  if (hasPermission === null) {
    return (
      <View style={[styles.container, { backgroundColor: isDarkMode ? '#222' : Constants.COLORS.BACKGROUND }]}>
        <View style={styles.content}>
          <ActivityIndicator size="large" color={isDarkMode ? '#fff' : Constants.COLORS.PRIMARY} />
          <Text style={[styles.loadingText, { color: isDarkMode ? Constants.COLORS.CARD : Constants.COLORS.SECONDARY_TEXT }]}>{t('loading')}</Text>
        </View>
      </View>
    );
  }

  if (hasPermission === false) {
    return (
      <View style={[styles.container, { backgroundColor: isDarkMode ? '#222' : Constants.COLORS.BACKGROUND }]}>
        <View style={styles.content}>
          <Text style={[styles.error, { color: Constants.COLORS.DESTRUCTIVE }]}>{t('error')}: Camera permission not granted</Text>
          <Pressable
            onPress={async () => {
              const { status } = await requestPermission();
              setHasPermission(status === 'granted');
            }}
            style={({ pressed }) => [
              styles.retryButton,
              { backgroundColor: isDarkMode ? '#555' : Constants.COLORS.PRIMARY, opacity: pressed ? 0.7 : 1 },
            ]}
            accessibilityLabel="Retry camera permission"
          >
            <Text style={styles.retryButtonLabel}>Retry Permission</Text>
          </Pressable>
        </View>
      </View>
    );
  }

  return (
    <View style={[styles.container, { backgroundColor: isDarkMode ? '#222' : Constants.COLORS.BACKGROUND }]}>
      <View style={styles.content}>
        {isCameraActive ? (
          <CameraView style={styles.camera} ref={cameraRef} facing="back" />
        ) : (
          <View style={[styles.cameraPlaceholder, { backgroundColor: isDarkMode ? '#444' : '#d3d3d3' }]}>
            <Text style={[styles.placeholderText, { color: isDarkMode ? Constants.COLORS.CARD : Constants.COLORS.SECONDARY_TEXT }]}>
              Camera Preview Will Appear Here
            </Text>
            <Text style={[styles.noteText, { color: isDarkMode ? Constants.COLORS.CARD : Constants.COLORS.SECONDARY_TEXT }]}>
              Note: ASL translation is not fully functional and requires TensorFlow/MediaPipe for accurate sign language recognition.
            </Text>
          </View>
        )}
        <Pressable
          onPress={isCameraActive ? stopCamera : startCamera}
          style={({ pressed }) => [
            isCameraActive ? styles.stopButton : styles.startButton,
            { backgroundColor: isCameraActive ? Constants.COLORS.DESTRUCTIVE : (isDarkMode ? '#555' : Constants.COLORS.PRIMARY), opacity: pressed ? 0.7 : 1 },
          ]}
          accessibilityLabel={isCameraActive ? 'Stop camera' : 'Start camera'}
        >
          <Text style={isCameraActive ? styles.stopButtonLabel : styles.startButtonLabel}>
            {isCameraActive ? t('stopCamera') : t('startCamera')}
          </Text>
        </Pressable>
        {error ? <Text style={[styles.error, { color: Constants.COLORS.DESTRUCTIVE }]}>{error}</Text> : null}
        {translatedText ? (
          <View style={[styles.resultContainer, { backgroundColor: isDarkMode ? '#333' : Constants.COLORS.CARD }]}>
            <Text style={[styles.resultLabel, { color: isDarkMode ? Constants.COLORS.CARD : Constants.COLORS.TEXT }]}>{t('translated')}</Text>
            <Text style={[styles.translated, { color: isDarkMode ? Constants.COLORS.CARD : Constants.COLORS.SECONDARY_TEXT }]}>{translatedText}</Text>
            <Pressable
              onPress={handleDeleteTranslation}
              style={({ pressed }) => [
                styles.deleteButton,
                { backgroundColor: Constants.COLORS.DESTRUCTIVE, opacity: pressed ? 0.7 : 1 },
              ]}
              accessibilityLabel="Delete translation"
            >
              <Text style={styles.deleteButtonLabel}>{t('deleteTranslation')}</Text>
            </Pressable>
          </View>
        ) : null}
      </View>
      <Toast
        message={getSafeMessage(error)}
        visible={toastVisible}
        onHide={() => setToastVisible(false)}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1 },
  content: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: Constants.SPACING.SECTION,
  },
  camera: {
    width: '100%',
    height: 300,
    marginBottom: Constants.SPACING.SECTION,
    borderRadius: 15,
    overflow: 'hidden',
  },
  cameraPlaceholder: {
    width: '100%',
    height: 300,
    marginBottom: Constants.SPACING.SECTION,
    justifyContent: 'center',
    alignItems: 'center',
    borderRadius: 15,
    shadowColor: Constants.COLORS.SHADOW,
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  placeholderText: {
    fontSize: Constants.FONT_SIZES.BODY,
    textAlign: 'center',
    fontWeight: '600',
  },
  noteText: {
    fontSize: Constants.FONT_SIZES.SECONDARY,
    textAlign: 'center',
    marginTop: Constants.SPACING.MEDIUM,
  },
  error: {
    fontSize: Constants.FONT_SIZES.SECONDARY,
    marginTop: Constants.SPACING.MEDIUM,
    marginBottom: Constants.SPACING.SECTION,
    textAlign: 'center',
  },
  loadingText: {
    marginTop: Constants.SPACING.MEDIUM,
    fontSize: Constants.FONT_SIZES.BODY,
    textAlign: 'center',
  },
  resultContainer: {
    marginTop: Constants.SPACING.SECTION,
    padding: Constants.SPACING.SECTION,
    borderRadius: 12,
    shadowColor: Constants.COLORS.SHADOW,
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
    width: '100%',
  },
  resultLabel: {
    fontSize: Constants.FONT_SIZES.BODY,
    fontWeight: 'bold',
    marginBottom: Constants.SPACING.MEDIUM,
    letterSpacing: 0.5,
  },
  translated: {
    fontSize: Constants.FONT_SIZES.BODY,
    marginBottom: Constants.SPACING.LARGE,
    lineHeight: 24,
  },
  startButton: {
    paddingVertical: 5,
    borderRadius: 10,
    marginBottom: Constants.SPACING.MEDIUM,
  },
  startButtonLabel: {
    fontSize: Constants.FONT_SIZES.BODY,
    fontWeight: 'bold',
    color: Constants.COLORS.CARD,
  },
  stopButton: {
    paddingVertical: 5,
    borderRadius: 10,
    marginBottom: Constants.SPACING.MEDIUM,
  },
  stopButtonLabel: {
    fontSize: Constants.FONT_SIZES.BODY,
    fontWeight: 'bold',
    color: Constants.COLORS.CARD,
  },
  deleteButton: {
    paddingVertical: 5,
    borderRadius: 10,
  },
  deleteButtonLabel: {
    fontSize: Constants.FONT_SIZES.BODY,
    fontWeight: 'bold',
    color: Constants.COLORS.CARD,
  },
  retryButton: {
    paddingVertical: 5,
    borderRadius: 10,
    marginTop: Constants.SPACING.MEDIUM,
  },
  retryButtonLabel: {
    fontSize: Constants.FONT_SIZES.BODY,
    fontWeight: 'bold',
    color: Constants.COLORS.CARD,
  },
});

export default ASLTranslationScreen;