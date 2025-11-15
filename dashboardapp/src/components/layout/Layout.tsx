// src/components/layout/Layout.tsx
import React, { useState } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import {
    Box,
    CssBaseline,
    Toolbar,
    AppBar,
    Typography,
    Button,
    Avatar,
    IconButton,
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import Sidebar from './Sidebar';
import { useAuth } from '../../contexts/AuthContext';
import LogoutDialog from '../LogoutDialog';

const Layout = () => {
    const [mobileOpen, setMobileOpen] = useState(false);
    const [logoutOpen, setLogoutOpen] = useState(false);
    const { user, logout } = useAuth();
    const navigate = useNavigate();

    const handleDrawerToggle = () => {
        setMobileOpen(!mobileOpen);
    };

    const handleLogoutClick = () => {
        setLogoutOpen(true);
    };

    const handleLogoutConfirm = () => {
        setLogoutOpen(false);
        logout();
    };

    const handleLogoutCancel = () => {
        setLogoutOpen(false);
    };

    return (
        <Box sx={{ display: 'flex' }}>
            <CssBaseline />
            <AppBar
                position="fixed"
                sx={{
                    width: { sm: `calc(100% - 240px)` },
                    ml: { sm: '240px' },
                }}
            >
                <Toolbar>
                    <IconButton
                        color="inherit"
                        aria-label="open drawer"
                        edge="start"
                        onClick={handleDrawerToggle}
                        sx={{ mr: 2, display: { sm: 'none' } }}
                    >
                        <MenuIcon />
                    </IconButton>
                    <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
                        IHM Distribution
                    </Typography>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <Typography variant="body1">
                            {user?.name}
                        </Typography>
                        <Avatar>{user?.name?.[0]}</Avatar>
                        <Button color="inherit" onClick={handleLogoutClick}>
                            Logout
                        </Button>
                    </Box>
                </Toolbar>
            </AppBar>
            <Sidebar mobileOpen={mobileOpen} handleDrawerToggle={handleDrawerToggle} />
            <Box
                component="main"
                sx={{
                    flexGrow: 1,
                    p: 3,
                    width: { sm: `calc(100% - 240px)` },
                    ml: { sm: '240px' },
                    mt: 8,
                }}
            >
                <Toolbar />
                <Outlet />
            </Box>
            <LogoutDialog
                open={logoutOpen}
                onClose={handleLogoutCancel}
                onConfirm={handleLogoutConfirm}
            />
        </Box>
    );
};

export default Layout;