// src/pages/NotFound.tsx
import React from 'react';
import { Link } from 'react-router-dom';
import { Box, Typography, Button, Container } from '@mui/material';

const NotFound = () => {
    return (
        <Container maxWidth="md">
            <Box
                sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    justifyContent: 'center',
                    minHeight: '80vh',
                    textAlign: 'center',
                }}
            >
                <Typography variant="h1" sx={{ fontSize: '6rem', fontWeight: 700, mb: 2 }}>
                    404
                </Typography>
                <Typography variant="h4" sx={{ mb: 3 }}>
                    Oops! Page not found
                </Typography>
                <Typography variant="body1" color="text.secondary" sx={{ mb: 4, maxWidth: '600px' }}>
                    The page you are looking for might have been removed, had its name changed, or is
                    temporarily unavailable.
                </Typography>
                <Button
                    component={Link}
                    to="/"
                    variant="contained"
                    color="primary"
                    size="large"
                >
                    Go to Homepage
                </Button>
            </Box>
        </Container>
    );
};

export default NotFound;