// =============================================================================
// File: product.model.ts
// Layer: Core — Models
// Purpose: TypeScript interfaces for product data models.
//          Mirrors the backend DTOs for type-safe API communication.
// =============================================================================

/**
 * Represents a product returned from the API.
 * Maps to the backend ProductDto record.
 */
export interface Product {
  /** Unique product identifier */
  id: number;
  /** Display name of the product */
  name: string;
  /** Detailed product description */
  description: string;
  /** Stock Keeping Unit identifier */
  sku: string;
  /** Product price */
  price: number;
  /** Current inventory quantity */
  quantity: number;
  /** Associated category ID */
  categoryId: number;
  /** Resolved category name (may be null if category not loaded) */
  categoryName?: string | null;
  /** ISO 8601 timestamp when product was created */
  createdAt: string;
  /** ISO 8601 timestamp when product was last updated */
  updatedAt: string;
}

/**
 * Payload for creating a new product.
 * Maps to the backend CreateProductDto record.
 */
export interface CreateProduct {
  name: string;
  description: string;
  sku: string;
  price: number;
  quantity: number;
  categoryId: number;
}

/**
 * Payload for updating an existing product.
 * Maps to the backend UpdateProductDto record.
 */
export interface UpdateProduct {
  name: string;
  description: string;
  sku: string;
  price: number;
  quantity: number;
  categoryId: number;
}

/**
 * Query parameters for the product listing endpoint.
 */
export interface ProductQueryParams {
  searchTerm?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  page?: number;
  pageSize?: number;
}
