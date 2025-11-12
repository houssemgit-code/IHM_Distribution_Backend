// src/App.tsx
import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { useAuth } from './contexts/AuthContext';
import Layout from './components/layout/Layout';
import Products from './pages/Products';
import AddProduct from './pages/AddProduct';
import EditProduct from './pages/EditProduct';
import Login from './pages/auth/Login';
import Register from './pages/auth/Register';
import ProtectedRoute from './components/layout/ProtectedRoute';
import NotFound from './pages/NotFound';
import theme from './theme';
import { Box, CircularProgress } from '@mui/material';

const AppContent = () => {
    const { isAuthenticated, loading } = useAuth();

    if (loading) {
        return (
            <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
                <CircularProgress />
            </Box>
        );
    }

    return (
        <Routes>
            <Route
                path="/login"
                element={isAuthenticated ? <Navigate to="/" replace /> : <Login />}
            />
            <Route
                path="/register"
                element={isAuthenticated ? <Navigate to="/" replace /> : <Register />}
            />
            <Route
                path="/"
                element={
                    <ProtectedRoute>
                        <Layout />
                    </ProtectedRoute>
                }
            >
                <Route index element={<div>Dashboard Content</div>} />
                <Route path="products">
                    <Route index element={<Products />} />
                    <Route path="add" element={<AddProduct />} />
                    <Route
                        path="edit/:id"
                        element={
                            <EditProduct
                                onSubmit={function (values: any): void | Promise<void> {
                                    throw new Error('Function not implemented.');
                                }}
                            />
                        }
                    />
                </Route>
            </Route>
            <Route path="*" element={<NotFound />} />
        </Routes>
    );
};

function App() {
    return (
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <AppContent />
        </ThemeProvider>
    );
}

export default App;
