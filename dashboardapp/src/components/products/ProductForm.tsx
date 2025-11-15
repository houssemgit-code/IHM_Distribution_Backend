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
import { Product, ProductFormData, UpdateProductDto } from '../../types/product';

interface ProductFormProps {
    initialValues?: Partial<Product>;
    onSubmit: (values: ProductFormData) => Promise<void> | void;
    onCancel: () => void;
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
    onCancel,
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

    const formik = useFormik<UpdateProductDto>({
        initialValues: {
            name: initialValues.name || '',
            description: initialValues.description || '',
            price: initialValues.price || 0,
            stockInWarehouse: initialValues.stockInWarehouse || 0,
        },
        validationSchema,
        enableReinitialize: true,
        onSubmit: async (values, { setSubmitting }) => {
            try {
                await onSubmit(values as ProductFormData);
            } finally {
                setSubmitting(false);
            }
        },
    });

    return (
        <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
            <Paper sx={{ p: 4 }}>
                <Typography variant="h5" gutterBottom>
                    {initialValues.id ? 'Edit Product' : 'Add New Product'}
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
                        onBlur={formik.handleBlur}
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
                        onBlur={formik.handleBlur}
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
                            onBlur={formik.handleBlur}
                            error={formik.touched.price && Boolean(formik.errors.price)}
                            helperText={formik.touched.price && formik.errors.price}
                            disabled={isSubmitting}
                            inputProps={{ min: 0, step: '0.01' }}
                        />

                        <TextField
                            fullWidth
                            id="stockInWarehouse"
                            name="stockInWarehouse"
                            label="Stock in Warehouse"
                            type="number"
                            value={formik.values.stockInWarehouse}
                            onChange={formik.handleChange}
                            onBlur={formik.handleBlur}
                            error={formik.touched.stockInWarehouse && Boolean(formik.errors.stockInWarehouse)}
                            helperText={formik.touched.stockInWarehouse && formik.errors.stockInWarehouse}
                            disabled={isSubmitting}
                            inputProps={{ min: 0 }}
                        />
                    </Box>

                    <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, mt: 2 }}>
                        <Button
                            variant="outlined"
                            onClick={onCancel}
                            disabled={isSubmitting}
                        >
                            Cancel
                        </Button>
                        <Button
                            type="submit"
                            variant="contained"
                            color="primary"
                            disabled={isSubmitting || formik.isSubmitting}
                            startIcon={isSubmitting ? <CircularProgress size={20} /> : null}
                        >
                            {isSubmitting ? 'Saving...' : initialValues.id ? 'Update' : 'Create'}
                        </Button>
                    </Box>
                </Box>
            </Paper>
        </Container>
    );
};

export default ProductForm;