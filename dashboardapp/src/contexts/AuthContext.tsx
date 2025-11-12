// src/contexts/AuthContext.tsx
import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { login as loginApi, getCurrentUser, register as registerApi, User } from '../services/api';
import { setToken, clearToken as clearStoredToken, getToken } from '../services/auth';

export interface AuthContextType {
    user: User | null;
    loading: boolean;
    login: (email: string, password: string) => Promise<void>;
    register: (name: string, email: string, password: string) => Promise<void>;
    logout: () => void;
    isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<User | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadUser = async () => {
            try {
                const token = getToken();
                if (token) {
                    const userData = await getCurrentUser();
                    setUser(userData);
                }
            } catch (error) {
                console.error('Failed to load user', error);
                clearStoredToken();
            } finally {
                setLoading(false);
            }
        };

        loadUser();
    }, []);

    const login = async (userEmail: string, pinCode: string) => {
        const { token, name, email, id } = await loginApi({ userEmail, pinCode });
        setToken(token);
        setUser({
            id,
            name,
            email: email,
            role: 'agent', // Default role, adjust as needed
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString()
        });
    };

    const register = async (name: string, email: string, password: string) => {
        const { token } = await registerApi({ name, email, password });
        setToken(token);
        setUser(user);
    };

    const logout = () => {
        clearStoredToken();
        setUser(null);
        window.location.href = '/login';
    };

    return (
        <AuthContext.Provider
            value={{
                user,
                loading,
                login,
                register,
                logout,
                isAuthenticated: !!user,
            }}
        >
            {!loading && children}
        </AuthContext.Provider>
    );
};

export const useAuth = (): AuthContextType => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};