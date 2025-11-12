import React from 'react';
import { Card, CardContent, Typography, CardActions, Button } from '@mui/material';
import { Product } from '../../types/product';

interface ProductCardProps {
  product: Product;
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
}

export const ProductCard: React.FC<ProductCardProps> = ({ product, onEdit, onDelete }) => {
  return (
    <Card sx={{ minWidth: 275, m: 1 }}>
      <CardContent>
        <Typography variant="h5" component="div">
          {product.name}
        </Typography>
        <Typography color="text.secondary">
          ${product.price}
        </Typography>
        <Typography variant="body2">
          {product.description}
        </Typography>
        <Typography variant="body2">
          Stock: {product.stockInWarehouse}
        </Typography>
      </CardContent>
      <CardActions>
        <Button size="small" onClick={() => onEdit(product.id)}>Edit</Button>
        <Button size="small" color="error" onClick={() => onDelete(product.id)}>Delete</Button>
      </CardActions>
    </Card>
  );
};

export default ProductCard;