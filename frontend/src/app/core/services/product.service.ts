// =============================================================================
// File: product.service.ts
// Layer: Core — Services
// Purpose: Angular service for all product API communications.
//          Uses HttpClient with RxJS for reactive API calls.
// =============================================================================

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResult } from '../models/api-response.model';
import { Product, CreateProduct, UpdateProduct, ProductQueryParams } from '../models/product.model';

/**
 * Service for product API operations.
 * Provides reactive Observable-based methods for all CRUD operations
 * and search functionality. Singleton service provided at root level.
 */
@Injectable({
  providedIn: 'root'
})
export class ProductService {
  /** Base URL for the products API endpoint */
  private readonly apiUrl = `${environment.apiUrl}/products`;

  constructor(private http: HttpClient) {}

  /**
   * Retrieves a paginated list of products with optional filtering.
   *
   * @param params - Query parameters for filtering, searching, and pagination
   * @returns Observable of PagedResult containing products
   */
  getProducts(params: ProductQueryParams = {}): Observable<PagedResult<Product>> {
    // Build HttpParams from the query parameter object
    let httpParams = new HttpParams();

    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    if (params.categoryId != null) httpParams = httpParams.set('categoryId', params.categoryId.toString());
    if (params.minPrice != null) httpParams = httpParams.set('minPrice', params.minPrice.toString());
    if (params.maxPrice != null) httpParams = httpParams.set('maxPrice', params.maxPrice.toString());
    if (params.page != null) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize != null) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http
      .get<ApiResponse<PagedResult<Product>>>(this.apiUrl, { params: httpParams })
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.error ?? 'Failed to load products');
          }
          return response.data;
        }),
        catchError(err => throwError(() => new Error(err.message ?? 'Network error')))
      );
  }

  /**
   * Retrieves a single product by its ID.
   *
   * @param id - The product ID
   * @returns Observable of the product
   */
  getProduct(id: number): Observable<Product> {
    return this.http
      .get<ApiResponse<Product>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.error ?? 'Product not found');
          }
          return response.data;
        }),
        catchError(err => throwError(() => new Error(err.error?.error ?? err.message ?? 'Network error')))
      );
  }

  /**
   * Creates a new product.
   *
   * @param product - The product creation payload
   * @returns Observable of the created product
   */
  createProduct(product: CreateProduct): Observable<Product> {
    return this.http
      .post<ApiResponse<Product>>(this.apiUrl, product)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.error ?? 'Failed to create product');
          }
          return response.data;
        }),
        catchError(err => throwError(() => new Error(err.error?.error ?? err.message ?? 'Network error')))
      );
  }

  /**
   * Updates an existing product.
   *
   * @param id - The ID of the product to update
   * @param product - The update payload
   * @returns Observable of the updated product
   */
  updateProduct(id: number, product: UpdateProduct): Observable<Product> {
    return this.http
      .put<ApiResponse<Product>>(`${this.apiUrl}/${id}`, product)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.error ?? 'Failed to update product');
          }
          return response.data;
        }),
        catchError(err => throwError(() => new Error(err.error?.error ?? err.message ?? 'Network error')))
      );
  }

  /**
   * Deletes a product by its ID.
   *
   * @param id - The ID of the product to delete
   * @returns Observable<void>
   */
  deleteProduct(id: number): Observable<void> {
    return this.http
      .delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.error ?? 'Failed to delete product');
          }
        }),
        catchError(err => throwError(() => new Error(err.error?.error ?? err.message ?? 'Network error')))
      );
  }

  /**
   * Performs a fuzzy search across products using the backend search engine.
   *
   * @param query - The search query string (supports typos)
   * @returns Observable of matching products ordered by relevance
   */
  searchProducts(query: string): Observable<Product[]> {
    const params = new HttpParams().set('q', query);
    return this.http
      .get<ApiResponse<Product[]>>(`${this.apiUrl}/search`, { params })
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.error ?? 'Search failed');
          }
          return response.data;
        }),
        catchError(err => throwError(() => new Error(err.error?.error ?? err.message ?? 'Network error')))
      );
  }
}
