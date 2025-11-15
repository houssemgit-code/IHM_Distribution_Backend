// src/components/layout/Sidebar.tsx
import React from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
    Drawer,
    List,
    ListItem,
    ListItemIcon,
    ListItemText,
    Divider,
    useTheme,
    useMediaQuery,
    Toolbar,
    Typography,
    Box,
} from '@mui/material';
import {
    Dashboard as DashboardIcon,
    Inventory as InventoryIcon,
    People as PeopleIcon,
    Receipt as ReceiptIcon,
    Settings as SettingsIcon,
} from '@mui/icons-material';

interface SidebarProps {
    mobileOpen: boolean;
    handleDrawerToggle: () => void;
}

const drawerWidth = 240;

const Sidebar: React.FC<SidebarProps> = ({ mobileOpen, handleDrawerToggle }) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

    const drawer = (
        <div>
            <Toolbar>
                <Typography variant="h6" noWrap component="div">
                    IHM Distribution
                </Typography>
            </Toolbar>
            <Divider />
            <List>
                <ListItem component={RouterLink} to="/" sx={{ cursor: 'pointer', '&:hover': { backgroundColor: 'action.hover' } }}>
                    <ListItemIcon><DashboardIcon /></ListItemIcon>
                    <ListItemText primary="Dashboard" />
                </ListItem>
                <ListItem component={RouterLink} to="/products" sx={{ cursor: 'pointer', '&:hover': { backgroundColor: 'action.hover' } }}>
                    <ListItemIcon><InventoryIcon /></ListItemIcon>
                    <ListItemText primary="Products" />
                </ListItem>
                <ListItem component={RouterLink} to="/clients" sx={{ cursor: 'pointer', '&:hover': { backgroundColor: 'action.hover' } }}>
                    <ListItemIcon><PeopleIcon /></ListItemIcon>
                    <ListItemText primary="Clients" />
                </ListItem>
                <ListItem component={RouterLink} to="/orders" sx={{ cursor: 'pointer', '&:hover': { backgroundColor: 'action.hover' } }}>
                    <ListItemIcon><ReceiptIcon /></ListItemIcon>
                    <ListItemText primary="Orders" />
                </ListItem>
            </List>
            <Divider />
            <List>
                <ListItem component={RouterLink} to="/settings" sx={{ cursor: 'pointer', '&:hover': { backgroundColor: 'action.hover' } }}>
                    <ListItemIcon><SettingsIcon /></ListItemIcon>
                    <ListItemText primary="Settings" />
                </ListItem>
            </List>
        </div>
    );

    return (
        <Box
            component="nav"
            sx={{ width: { sm: drawerWidth }, flexShrink: { sm: 0 } }}
            aria-label="mailbox folders"
        >
            {/* Mobile drawer */}
            <Drawer
                variant="temporary"
                open={mobileOpen}
                onClose={handleDrawerToggle}
                ModalProps={{
                    keepMounted: true, // Better open performance on mobile.
                }}
                sx={{
                    display: { xs: 'block', sm: 'none' },
                    '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
                }}
            >
                {drawer}
            </Drawer>
            {/* Desktop drawer */}
            <Drawer
                variant="permanent"
                sx={{
                    display: { xs: 'none', sm: 'block' },
                    '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
                }}
                open
            >
                {drawer}
            </Drawer>
        </Box>
    );
};

export default Sidebar;