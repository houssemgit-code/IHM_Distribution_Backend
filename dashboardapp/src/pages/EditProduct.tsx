// src/pages/EditProduct.tsx
import React from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import {
    Box,
    Button,
    TextField,
    CircularProgress,
    Container,
    Paper,
    Typography,
} from '@mui/material';
import { Product } from '../types/product';

interface ProductFormProps {
    initialValues?: Partial<Product>;
    onSubmit: (values: any) => Promise<void> | void;
    isSubmitting?: boolean;
}

const ProductForm: React.FC<ProductFormProps> = ({
    initialValues = {
        name: '',
        description: '',
        price: 0,
        stockInWarehouse: 0,
    },
    onSubmit,
    isSubmitting = false,
}) => {
    const validationSchema = Yup.object({
        name: Yup.string().required('Name is required'),
        description: Yup.string(),
        price: Yup.number()
            .required('Price is required')
            .positive('Price must be positive'),
        stockInWarehouse: Yup.number()
            .required('Stock is required')
            .min(0, 'Stock cannot be negative')
            .integer('Stock must be a whole number'),
    });

    const formik = useFormik({
        initialValues,
        validationSchema,
        enableReinitialize: true,
        onSubmit,
    });

    return (
        <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
            <Paper sx={{ p: 4 }}>
                <Typography variant="h5" gutterBottom>
                    Edit Product
                </Typography>

                <Box
                    component="form"
                    onSubmit={formik.handleSubmit}
                    sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}
                >
                    <TextField
                        fullWidth
                        id="name"
                        name="name"
                        label="Product Name"
                        value={formik.values.name}
                        onChange={formik.handleChange}
                        error={formik.touched.name && Boolean(formik.errors.name)}
                        helperText={formik.touched.name && formik.errors.name}
                        disabled={isSubmitting}
                    />

                    <TextField
                        fullWidth
                        id="description"
                        name="description"
                        label="Description"
                        multiline
                        rows={4}
                        value={formik.values.description}
                        onChange={formik.handleChange}
                        error={formik.touched.description && Boolean(formik.errors.description)}
                        helperText={formik.touched.description && formik.errors.description}
                        disabled={isSubmitting}
                    />

                    <Box sx={{ display: 'flex', gap: 3 }}>
                        <TextField
                            fullWidth
                            id="price"
                            name="price"
                            label="Price"
                            type="number"
                            value={formik.values.price}
                            onChange={formik.handleChange}
                            error={formik.touched.price && Boolean(formik.errors.price)}
                            helperText={formik.touched.price && formik.errors.price}
                            disabled={isSubmitting}
                        />

                        <TextField
                            fullWidth
                            id="stockInWarehouse"
                            name="stockInWarehouse"
                            label="Stock in Warehouse"
                            type="number"
                            value={formik.values.stockInWarehouse}
                            onChange={formik.handleChange}
                            error={formik.touched.stockInWarehouse && Boolean(formik.errors.stockInWarehouse)}
                            helperText={formik.touched.stockInWarehouse && formik.errors.stockInWarehouse}
                            disabled={isSubmitting}
                        />
                    </Box>

                    <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 2 }}>
                        <Button
                            type="submit"
                            variant="contained"
                            color="primary"
                            disabled={isSubmitting}
                            startIcon={isSubmitting ? <CircularProgress size={20} /> : null}
                        >
                            {isSubmitting ? 'Saving...' : 'Save Changes'}
                        </Button>
                    </Box>
                </Box>
            </Paper>
        </Container>
    );
};

export default ProductForm;