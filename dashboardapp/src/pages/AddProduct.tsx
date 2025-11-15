// src/pages/AddProduct.tsx
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Alert, Box, Button, Typography } from '@mui/material';

import ProductForm from '../components/products/ProductForm';
import { createProduct, uploadProductImage } from '../services/api';
import { ProductFormData } from '../types/product';

const AddProduct: React.FC = () => {
    const navigate = useNavigate();

    const [error, setError] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [selectedImage, setSelectedImage] = useState<File | null>(null);

    const handleSubmit = async (values: ProductFormData) => {
        try {
            setIsSubmitting(true);
            setError('');

            const product = await createProduct(values);

            if (selectedImage && product?.id) {
                await uploadProductImage(product.id, selectedImage);
            }

            navigate('/products');
        } catch (err) {
            console.error('Error creating product:', err);
            setError('Failed to create product. Please try again.');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            setSelectedImage(e.target.files[0]);
        }
    };

    return (
        <div>
            <Typography variant="h4" sx={{ mb: 2 }}>
                Add New Product
            </Typography>

            {error && (
                <Alert severity="error" sx={{ mb: 3 }}>
                    {error}
                </Alert>
            )}

            <ProductForm
                initialValues={{
                    name: '',
                    description: '',
                    price: 0,
                    stockInWarehouse: 0,
                }}
                onSubmit={handleSubmit}
                onCancel={() => navigate('/products')}
                isSubmitting={isSubmitting}
            />

            <Box sx={{ mt: -3, ml: 4 }}>
                <input
                    accept="image/*"
                    style={{ display: 'none' }}
                    id="product-image-upload"
                    type="file"
                    onChange={handleImageChange}
                    disabled={isSubmitting}
                />
                <label htmlFor="product-image-upload">
                    <Button variant="outlined" component="span" disabled={isSubmitting}>
                        {selectedImage ? 'Change Image' : 'Upload Image'}
                    </Button>
                </label>

                {selectedImage && (
                    <Typography variant="body2" sx={{ mt: 1 }}>
                        {selectedImage.name}
                    </Typography>
                )}
            </Box>
        </div>
    );
};

export default AddProduct;
