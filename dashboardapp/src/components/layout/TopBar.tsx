// src/components/layout/TopBar.tsx
import React from 'react';
import { AppBar, Toolbar, Typography, IconButton, Avatar } from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';

const TopBar = () => {
    return (
        <AppBar
            position="fixed"
            sx={{
                zIndex: (theme) => theme.zIndex.drawer + 1,
                backgroundColor: 'white',
                color: 'text.primary',
                boxShadow: '0 1px 3px rgba(0,0,0,0.12)',
            }}
        >
            <Toolbar>
                <IconButton
                    color="inherit"
                    aria-label="open drawer"
                    edge="start"
                    sx={{ mr: 2, display: { sm: 'none' } }}
                >
                    <MenuIcon />
                </IconButton>
                <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
                    IHM Distribution
                </Typography>
                <Avatar sx={{ bgcolor: 'primary.main' }}>U</Avatar>
            </Toolbar>
        </AppBar>
    );
};

export default TopBar;