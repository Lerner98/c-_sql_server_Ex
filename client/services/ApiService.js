import axios from 'axios';
import Constants from '../utils/Constants';
import AsyncStorageUtils from '../utils/AsyncStorage';

const API_URL = Constants.API_URL;

console.log('[ApiService] API_URL:', API_URL);

class ApiService {
  static instance;
  axiosInstance;

  constructor() {
    this.axiosInstance = axios.create({
      baseURL: API_URL,
      timeout: Constants.API_TIMEOUT,
    });

    // ✅ PROFESSIONAL: Clean, focused error interceptor
    this.axiosInstance.interceptors.response.use(
      (response) => response,
      async (error) => {
        const status = error?.response?.status;
        
        // Only clear session on actual authentication failures
        if (status === 401 || status === 403) {
          console.log('[ApiService] Authentication error, clearing session');
          await AsyncStorageUtils.removeItem('user');
          await AsyncStorageUtils.removeItem('signed_session_id');
          await AsyncStorageUtils.removeItem('preferences');
        }
        
        return Promise.reject(error);
      }
    );
  }

  static getInstance() {
    if (!ApiService.instance) {
      ApiService.instance = new ApiService();
    }
    return ApiService.instance;
  }

  /**
   * Make a GET request to the API with streamlined error handling.
   * @param {string} url - The API endpoint URL.
   * @param {string|null} token - The Bearer token for authentication (optional).
   * @param {Object} [config={}] - Additional axios configuration (optional).
   * @param {Object} [config.headers] - Custom headers for the request.
   * @returns {Promise<Object>} The API response with { success, data } or { success, error }.
   */
  async get(url, token = null, config = {}) {
    try {
      const headers = token ? { Authorization: `Bearer ${token}` } : {};
      
      console.log('[ApiService GET Request]', {
        fullUrl: `${this.axiosInstance.defaults.baseURL}${url}`,
        hasToken: !!token,
        timestamp: new Date().toISOString(),
      });

      const response = await this.axiosInstance.get(url, {
        ...config,
        headers: { ...headers, ...config.headers },
      });

      console.log('[ApiService GET Response]', {
        status: response.status,
        success: true,
        timestamp: new Date().toISOString(),
      });

      return { success: true, data: response.data };
    } catch (error) {
      console.log('[ApiService GET Error]', {
        message: error.message,
        status: error.response?.status,
        url: `${this.axiosInstance.defaults.baseURL}${url}`,
        timestamp: new Date().toISOString(),
      });

      return this.handleError(error);
    }
  }

  /**
   * Make a POST request to the API with streamlined error handling.
   * @param {string} url - The API endpoint URL.
   * @param {Object|FormData} data - The request payload (JSON or FormData).
   * @param {string|null} token - The Bearer token for authentication (optional).
   * @param {Object} [config={}] - Additional axios configuration (optional).
   * @param {Object} [config.headers] - Custom headers for the request.
   * @returns {Promise<Object>} The API response with { success, data } or { success, error }.
   */
  async post(url, data, token = null, config = {}) {
    try {
      const isFormData = typeof FormData !== 'undefined' && data instanceof FormData;
      
      const headers = {
        ...(token && { Authorization: `Bearer ${token}` }),
        ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
        ...config.headers,
      };

      console.log('[ApiService POST Request]', {
        fullUrl: `${this.axiosInstance.defaults.baseURL}${url}`,
        hasToken: !!token,
        isFormData,
        timestamp: new Date().toISOString(),
      });

      const response = await this.axiosInstance.post(url, data, {
        ...config,
        headers,
      });

      console.log('[ApiService POST Response]', {
        status: response.status,
        success: true,
        timestamp: new Date().toISOString(),
      });

      return { success: true, data: response.data };
    } catch (error) {
      console.log('[ApiService POST Error]', {
        message: error.message,
        status: error.response?.status,
        url: `${this.axiosInstance.defaults.baseURL}${url}`,
        timestamp: new Date().toISOString(),
      });

      return this.handleError(error);
    }
  }

  /**
   * Make a DELETE request to the API with streamlined error handling.
   * @param {string} url - The API endpoint URL.
   * @param {string|null} token - The Bearer token for authentication (optional).
   * @param {Object} [config={}] - Additional axios configuration (optional).
   * @param {Object} [config.headers] - Custom headers for the request.
   * @returns {Promise<Object>} The API response with { success, data } or { success, error }.
   */
  async delete(url, token = null, config = {}) {
    try {
      const headers = token ? { Authorization: `Bearer ${token}` } : {};
      
      console.log('[ApiService DELETE Request]', {
        fullUrl: `${this.axiosInstance.defaults.baseURL}${url}`,
        hasToken: !!token,
        timestamp: new Date().toISOString(),
      });

      const response = await this.axiosInstance.delete(url, {
        ...config,
        headers: { ...headers, ...config.headers },
      });

      console.log('[ApiService DELETE Response]', {
        status: response.status,
        success: true,
        timestamp: new Date().toISOString(),
      });

      return { success: true, data: response.data };
    } catch (error) {
      console.log('[ApiService DELETE Error]', {
        message: error.message,
        status: error.response?.status,
        url: `${this.axiosInstance.defaults.baseURL}${url}`,
        timestamp: new Date().toISOString(),
      });

      return this.handleError(error);
    }
  }

  /**
   * Handle API request errors and format them consistently.
   * ✅ PROFESSIONAL: Simplified error handling without verbose logging
   * @param {Object} error - The error object from axios.
   * @returns {Object} The formatted error with { success: false, error }.
   */
  handleError(error) {
    if (error.response) {
      // Server responded with error status
      return {
        success: false,
        error: error.response.data?.error || Constants.ERROR_MESSAGES.API_REQUEST_FAILED,
      };
    } else if (error.request) {
      // Network error - no response received
      return {
        success: false,
        error: Constants.ERROR_MESSAGES.API_NETWORK_ERROR,
      };
    } else {
      // Request setup error
      return {
        success: false,
        error: error.message || Constants.ERROR_MESSAGES.API_REQUEST_SETUP_ERROR,
      };
    }
  }
}

export default ApiService.getInstance();