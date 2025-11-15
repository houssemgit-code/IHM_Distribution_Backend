import React from 'react';
import { Box, Typography, Paper } from '@mui/material';

const Dashboard = () => {
    return (
        <Box sx={{ p: 3 }}>
            <Typography variant="h4" gutterBottom>
                Dashboard
            </Typography>
            <Paper sx={{ p: 3 }}>
                <Typography>Welcome to the Admin Dashboard</Typography>
                {/* Add your dashboard content here */}
            </Paper>
        </Box>
    );
};

export default Dashboard;