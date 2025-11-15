// src/types/product.ts
export interface ProductImage {
    id: string;
    imageUrl: string;
    displayOrder: number;
    createdAt: string;
}

export interface Product {
    id: string;
    name: string;
    description?: string;
    price: number;
    stockInWarehouse: number;
    images: ProductImage[];
    createdAt: string;
    updatedAt: string;
}

export interface CreateProductDto {
    name: string;
    description?: string;
    price: number;
    stockInWarehouse: number;
}

export interface UpdateProductDto extends Partial<CreateProductDto> { }

export interface CreateProductRequest {
    name: string;
    description?: string;
    price: number;
    stockInWarehouse: number;
    images?: File[];
}

export interface ProductFormData {
    name: string;
    description: string;
    price: number;  // string to handle form input
    stockInWarehouse: number;  // string to handle form input
    images?: ProductImage[] | null;  // Optional for file uploads
}