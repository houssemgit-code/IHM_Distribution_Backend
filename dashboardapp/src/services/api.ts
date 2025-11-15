// src/services/api.ts
import axios, { AxiosError, AxiosRequestConfig } from 'axios';
import { getToken, clearToken } from './auth';
import { Product } from '../types/product';


// Types
export interface User {
    id: string;
    name: string;
    email: string;
    role: string;
    createdAt: string;
    updatedAt: string;
}

interface LoginResponse {
    token: string;
    name: string;
    email: string;
    expiration: string;
    id: string;
}


const API_URL = 'http://localhost:7001/api'; // Update with your backend URL

// Create axios instance
const api = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request interceptor to add auth token
api.interceptors.request.use(
    (config) => {
        const token = getToken();
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Response interceptor to handle 401 Unauthorized
api.interceptors.response.use(
    (response) => response,
    (error: AxiosError) => {
        if (error.response?.status === 401) {
            // Clear token and redirect to login if token is invalid/expired
            clearToken();
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

// Auth API
export const login = async (credentials: {
    userEmail: string;
    pinCode: string
}): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/Auth/login', {
        userEmail: credentials.userEmail,
        PinCode: credentials.pinCode
    });
    return response.data;
};

export const register = async (userData: {
    name: string;
    email: string;
    password: string;
}): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/register', userData);
    return response.data;
};

export const getCurrentUser = async (): Promise<User> => {
    const response = await api.get<User>('/auth/me');
    return response.data;
};

// Products API
export const getProducts = async (): Promise<Product[]> => {
    const response = await api.get<Product[]>('/products');
    return response.data;
};

export const getProduct = async (id: string) => {
    const response = await api.get(`/products/${id}`);
    return response.data;
};

export const createProduct = async (productData: any) => {
    const response = await api.post('/products', productData);
    return response.data;
};

export const updateProduct = async (id: string, productData: any) => {
    const response = await api.put(`/products/${id}`, productData);
    return response.data;
};

export const deleteProduct = async (id: string) => {
    const response = await api.delete(`/products/${id}`);
    return response.data;
};

export const uploadProductImage = async (productId: string, file: File) => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await api.post(
        `/products/${productId}/images`,
        formData,
        {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        }
    );
    return response.data;
};

// Add this to your existing api.ts file
export const deleteProductImage = async (imageId: string) => {
    const response = await api.delete(`/products/images/${imageId}`);
    return response.data;
};

export default api;