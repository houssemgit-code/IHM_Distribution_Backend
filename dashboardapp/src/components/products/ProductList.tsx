// src/components/products/ProductList.tsx
import React from 'react';
import { Box, Typography, CircularProgress } from '@mui/material';
import { Product } from '../../types/product';
import ProductCard from './ProductCard';

interface ProductListProps {
    products: Product[];
    loading: boolean;
    onEdit: (id: string) => void;
    onDelete: (id: string) => void;
}

export const ProductList: React.FC<ProductListProps> = ({
    products,
    loading,
    onEdit,
    onDelete
}) => {
    if (loading) {
        return (
            <Box display="flex" justifyContent="center" p={4}>
                <CircularProgress />
            </Box>
        );
    }

    if (products.length === 0) {
        return (
            <Box p={4} textAlign="center">
                <Typography variant="h6" color="textSecondary">
                    No products found
                </Typography>
            </Box>
        );
    }

    return (
        <Box
            sx={{
                display: 'grid',
                gridTemplateColumns: {
                    xs: '1fr',
                    sm: 'repeat(2, 1fr)',
                    md: 'repeat(3, 1fr)',
                    lg: 'repeat(4, 1fr)',
                },
                gap: 3,
                p: 2
            }}
        >
            {products.map((product) => (
                <Box key={product.id}>
                    <ProductCard
                        product={product}
                        onEdit={onEdit}
                        onDelete={onDelete}
                    />
                </Box>
            ))}
        </Box>
    );
};