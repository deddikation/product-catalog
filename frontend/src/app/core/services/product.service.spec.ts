// =============================================================================
// File: product.service.spec.ts
// Layer: Frontend Tests
// Purpose: Unit tests for the ProductService.
//          Uses HttpClientTestingModule to intercept and verify HTTP calls.
// =============================================================================

import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';

import { ProductService } from './product.service';
import { ApiResponse, PagedResult } from '../models/api-response.model';
import { Product } from '../models/product.model';
import { environment } from '../../../environments/environment';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;

  const apiUrl = `${environment.apiUrl}/products`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ProductService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    // Verify no unexpected HTTP requests were made
    httpMock.verify();
  });

  /**
   * Service should be created.
   */
  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  /**
   * getProducts should call GET /api/products.
   */
  it('should GET products with correct URL', () => {
    const mockResponse: ApiResponse<PagedResult<Product>> = {
      success: true,
      data: {
        items: [{ id: 1, name: 'Laptop', description: '', sku: 'LP01', price: 999, quantity: 5, categoryId: 1, createdAt: '', updatedAt: '' }],
        totalCount: 1,
        page: 1,
        pageSize: 10,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false
      },
      error: null
    };

    service.getProducts({ page: 1, pageSize: 10 }).subscribe(result => {
      expect(result.items.length).toBe(1);
      expect(result.items[0].name).toBe('Laptop');
    });

    const req = httpMock.expectOne(r => r.url === apiUrl);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  /**
   * getProduct should call GET /api/products/:id.
   */
  it('should GET single product by id', () => {
    const mockProduct: Product = { id: 1, name: 'Laptop', description: '', sku: 'LP01', price: 999, quantity: 5, categoryId: 1, createdAt: '', updatedAt: '' };
    const mockResponse: ApiResponse<Product> = { success: true, data: mockProduct, error: null };

    service.getProduct(1).subscribe(p => {
      expect(p.id).toBe(1);
      expect(p.name).toBe('Laptop');
    });

    const req = httpMock.expectOne(`${apiUrl}/1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  /**
   * createProduct should call POST /api/products.
   */
  it('should POST to create a product', () => {
    const createPayload = { name: 'New Product', description: '', sku: 'NP01', price: 49.99, quantity: 10, categoryId: 1 };
    const createdProduct: Product = { id: 99, ...createPayload, createdAt: '', updatedAt: '' };
    const mockResponse: ApiResponse<Product> = { success: true, data: createdProduct, error: null };

    service.createProduct(createPayload).subscribe(p => {
      expect(p.id).toBe(99);
      expect(p.name).toBe('New Product');
    });

    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body.name).toBe('New Product');
    req.flush(mockResponse);
  });

  /**
   * deleteProduct should call DELETE /api/products/:id.
   */
  it('should DELETE a product by id', () => {
    const mockResponse: ApiResponse<boolean> = { success: true, data: true, error: null };

    service.deleteProduct(1).subscribe(() => {
      // No error means success
    });

    const req = httpMock.expectOne(`${apiUrl}/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(mockResponse);
  });

  /**
   * searchProducts should call GET /api/products/search?q=...
   */
  it('should GET search results with query parameter', () => {
    const mockResponse: ApiResponse<Product[]> = {
      success: true,
      data: [{ id: 1, name: 'Laptop', description: '', sku: 'LP01', price: 999, quantity: 5, categoryId: 1, createdAt: '', updatedAt: '' }],
      error: null
    };

    service.searchProducts('laptop').subscribe(results => {
      expect(results.length).toBe(1);
    });

    const req = httpMock.expectOne(r => r.url === `${apiUrl}/search` && r.params.get('q') === 'laptop');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });
});
