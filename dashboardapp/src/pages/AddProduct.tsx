// src/pages/AddProduct.tsx
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Container,
    Typography,
    TextField,
    Button,
    Paper,
    Box,
    CircularProgress,
    Alert,
} from '@mui/material';
import { createProduct, uploadProductImage } from '../services/api';

interface ProductForm {
    name: string;
    description: string;
    price: string;
    stockInWarehouse: string;
}

const AddProduct = () => {
    const navigate = useNavigate();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState('');
    const [selectedImage, setSelectedImage] = useState<File | null>(null);
    const [formData, setFormData] = useState<ProductForm>({
        name: '',
        description: '',
        price: '',
        stockInWarehouse: ''
    });
    const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

    const validateForm = (): boolean => {
        const errors: Record<string, string> = {};

        if (!formData.name.trim()) {
            errors.name = 'Name is required';
        }

        if (!formData.price.trim()) {
            errors.price = 'Price is required';
        } else if (parseFloat(formData.price) <= 0) {
            errors.price = 'Price must be positive';
        }

        if (!formData.stockInWarehouse.trim()) {
            errors.stockInWarehouse = 'Stock is required';
        } else if (parseInt(formData.stockInWarehouse) < 0) {
            errors.stockInWarehouse = 'Stock cannot be negative';
        }

        setFieldErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
        // Clear error when user starts typing
        if (fieldErrors[name]) {
            setFieldErrors(prev => ({
                ...prev,
                [name]: ''
            }));
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        try {
            setIsSubmitting(true);
            setError('');

            // Prepare product data
            const productData = {
                name: formData.name.trim(),
                description: formData.description.trim(),
                price: parseFloat(formData.price),
                stockInWarehouse: parseInt(formData.stockInWarehouse),
                images: [] as string[]
            };

            // Create product
            const product = await createProduct(productData);

            // Upload image if selected
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
        <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
            <Paper sx={{ p: 4 }}>
                <Typography variant="h5" gutterBottom>
                    Add New Product
                </Typography>

                {error && (
                    <Alert severity="error" sx={{ mb: 3 }}>
                        {error}
                    </Alert>
                )}

                <form onSubmit={handleSubmit}>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                        <TextField
                            fullWidth
                            label="Product Name"
                            name="name"
                            value={formData.name}
                            onChange={handleInputChange}
                            error={Boolean(fieldErrors.name)}
                            helperText={fieldErrors.name}
                            disabled={isSubmitting}
                        />

                        <TextField
                            fullWidth
                            label="Description"
                            name="description"
                            multiline
                            rows={4}
                            value={formData.description}
                            onChange={handleInputChange}
                            error={Boolean(fieldErrors.description)}
                            helperText={fieldErrors.description}
                            disabled={isSubmitting}
                        />

                        <Box sx={{ display: 'flex', gap: 3 }}>
                            <TextField
                                fullWidth
                                label="Price"
                                name="price"
                                type="number"
                                value={formData.price}
                                onChange={handleInputChange}
                                error={Boolean(fieldErrors.price)}
                                helperText={fieldErrors.price}
                                disabled={isSubmitting}
                            />

                            <TextField
                                fullWidth
                                label="Stock in Warehouse"
                                name="stockInWarehouse"
                                type="number"
                                value={formData.stockInWarehouse}
                                onChange={handleInputChange}
                                error={Boolean(fieldErrors.stockInWarehouse)}
                                helperText={fieldErrors.stockInWarehouse}
                                disabled={isSubmitting}
                            />
                        </Box>

                        <Box>
                            <input
                                accept="image/*"
                                style={{ display: 'none' }}
                                id="product-image-upload"
                                type="file"
                                onChange={handleImageChange}
                                disabled={isSubmitting}
                            />
                            <label htmlFor="product-image-upload">
                                <Button
                                    variant="outlined"
                                    component="span"
                                    disabled={isSubmitting}
                                >
                                    {selectedImage ? 'Change Image' : 'Upload Image'}
                                </Button>
                            </label>
                            {selectedImage && (
                                <Typography variant="body2" sx={{ mt: 1 }}>
                                    {selectedImage.name}
                                </Typography>
                            )}
                        </Box>

                        <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, mt: 2 }}>
                            <Button
                                variant="outlined"
                                onClick={() => navigate('/products')}
                                disabled={isSubmitting}
                            >
                                Cancel
                            </Button>
                            <Button
                                type="submit"
                                variant="contained"
                                disabled={isSubmitting}
                            >
                                {isSubmitting ? <CircularProgress size={24} /> : 'Save Product'}
                            </Button>
                        </Box>
                    </Box>
                </form>
            </Paper>
        </Container>
    );
};

export default AddProduct;