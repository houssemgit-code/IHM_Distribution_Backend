// src/pages/EditProduct.tsx
import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Alert, Box, Button, CircularProgress, Typography, IconButton } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import Grid from '@mui/material/Grid';
import ProductForm from '../components/products/ProductForm';
import { getProduct, updateProduct, uploadProductImage, deleteProductImage } from "../services/api";
import { Product, ProductFormData } from '../types/product';

const EditProduct: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [product, setProduct] = useState<Product | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [selectedImages, setSelectedImages] = useState<File[]>([]);

    useEffect(() => {
        const load = async () => {
            try {
                if (!id) return;
                const data = await getProduct(id);
                setProduct(data);
            } catch (err) {
                console.error('Failed to load product:', err);
                setError('Failed to load product.');
            } finally {
                setLoading(false);
            }
        };
        load();
    }, [id]);

    const handleSubmit = async (values: ProductFormData) => {
        if (!id) return;
        try {
            setIsSubmitting(true);
            setError('');

            await updateProduct(id, { ...values, id });

            // Upload new images
            for (const file of selectedImages) {
                await uploadProductImage(id, file);
            }

            navigate('/products');
        } catch (err) {
            console.error('Error updating product:', err);
            setError('Failed to update product.');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setSelectedImages([...selectedImages, ...Array.from(e.target.files)]);
        }
    };

    const handleDeleteExistingImage = async (imageId: string) => {
        if (!id) return;
        try {
            await deleteProductImage(imageId);
            setProduct(prev => prev ? { ...prev, images: prev.images.filter(img => img.id !== imageId) } : prev);
        } catch (err) {
            console.error('Failed to delete image:', err);
            setError('Failed to delete image.');
        }
    };

    const handleDeleteNewImage = (index: number) => {
        setSelectedImages(prev => prev.filter((_, i) => i !== index));
    };

    if (loading) return <Box display="flex" justifyContent="center" mt={5}><CircularProgress /></Box>;
    if (!product) return <Alert severity="error">Product not found.</Alert>;

    return (
        <div>
            <Typography variant="h4" sx={{ mb: 2 }}>Edit Product</Typography>

            {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

            <ProductForm
                initialValues={{
                    name: product.name,
                    description: product.description || '',
                    price: Number(product.price),
                    stockInWarehouse: product.stockInWarehouse,
                }}
                onSubmit={handleSubmit}
                onCancel={() => navigate('/products')}
                isSubmitting={isSubmitting}
            />

            {/* Existing Images */}
            <Box sx={{ mt: 4 }}>
                <Typography variant="h6">Existing Images</Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, mt: 1 }}>
                    {product.images.map(img => (
                        <Box key={img.id} sx={{ position: 'relative', width: 120, height: 120 }}>
                            <img
                                src={img.imageUrl}
                                alt="product"
                                style={{ width: '100%', height: '100%', objectFit: 'cover', borderRadius: 4 }}
                            />
                            <IconButton
                                size="small"
                                color="error"
                                sx={{ position: 'absolute', top: 0, right: 0 }}
                                onClick={() => handleDeleteExistingImage(img.id)}
                            >
                                <DeleteIcon fontSize="small" />
                            </IconButton>
                        </Box>
                    ))}
                </Box>
            </Box>

            {/* New Images */}
            <Box sx={{ mt: 4 }}>
                <Typography variant="h6">Add New Images</Typography>
                <input
                    accept="image/*"
                    style={{ display: 'none' }}
                    id="product-image-upload"
                    type="file"
                    multiple
                    onChange={handleImageChange}
                />
                <label htmlFor="product-image-upload">
                    <Button variant="outlined" component="span" sx={{ mt: 1 }}>
                        Upload Images
                    </Button>
                </label>

                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, mt: 1 }}>
                    {selectedImages.map((file, index) => (
                        <Box key={index} sx={{ position: 'relative', width: 120, height: 120 }}>
                            <img
                                src={URL.createObjectURL(file)}
                                alt="new"
                                style={{ width: '100%', height: '100%', objectFit: 'cover', borderRadius: 4 }}
                            />
                            <IconButton
                                size="small"
                                color="error"
                                sx={{ position: 'absolute', top: 0, right: 0 }}
                                onClick={() => handleDeleteNewImage(index)}
                            >
                                <DeleteIcon fontSize="small" />
                            </IconButton>
                        </Box>
                    ))}
                </Box>
            </Box>
        </div>
    );
};

export default EditProduct;
