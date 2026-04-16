// =============================================================================
// File: category.service.ts
// Layer: Core — Services
// Purpose: Angular service for category API communications.
//          Handles flat list, tree retrieval, and category creation.
// =============================================================================

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Category, CategoryTree, CreateCategory } from '../models/category.model';

/**
 * Service for category API operations.
 * Provides methods to retrieve the flat category list, the hierarchical tree,
 * and to create new categories.
 */
@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  /** Base URL for the categories API endpoint */
  private readonly apiUrl = `${environment.apiUrl}/categories`;

  constructor(private http: HttpClient) {}

  /**
   * Retrieves all categories as a flat list.
   * Used for dropdowns and simple listings.
   *
   * @returns Observable of all categories
   */
  getCategories(): Observable<Category[]> {
    return this.http
      .get<ApiResponse<Category[]>>(this.apiUrl)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.error ?? 'Failed to load categories');
          }
          return response.data;
        }),
        catchError(err => throwError(() => new Error(err.error?.error ?? err.message ?? 'Network error')))
      );
  }

  /**
   * Retrieves the full category hierarchy as a tree.
   * The tree response uses custom JSON fields (depth, childCount, subcategories)
   * added by the backend's CategoryTreeJsonConverter.
   *
   * @returns Observable of root-level category tree nodes
   */
  getCategoryTree(): Observable<CategoryTree[]> {
    return this.http
      .get<CategoryTree[]>(`${this.apiUrl}/tree`)
      .pipe(
        catchError(err => throwError(() => new Error(err.error?.error ?? err.message ?? 'Network error')))
      );
  }

  /**
   * Creates a new category.
   *
   * @param category - The category creation payload
   * @returns Observable of the created category
   */
  createCategory(category: CreateCategory): Observable<Category> {
    return this.http
      .post<ApiResponse<Category>>(this.apiUrl, category)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.error ?? 'Failed to create category');
          }
          return response.data;
        }),
        catchError(err => throwError(() => new Error(err.error?.error ?? err.message ?? 'Network error')))
      );
  }
}
