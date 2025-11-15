// src/components/LoadingButton.tsx
import React from 'react';
import { Button, ButtonProps, CircularProgress } from '@mui/material';

interface LoadingButtonProps extends ButtonProps {
    loading?: boolean;
    loadingText?: string;
}

const LoadingButton: React.FC<LoadingButtonProps> = ({
    loading = false,
    loadingText = 'Loading...',
    children,
    disabled,
    startIcon,
    ...props
}) => {
    return (
        <Button
            disabled={disabled || loading}
            startIcon={
                loading ? (
                    <CircularProgress size={20} color="inherit" />
                ) : (
                    startIcon
                )
            }
            {...props}
        >
            {loading && loadingText ? loadingText : children}
        </Button>
    );
};

export default LoadingButton;