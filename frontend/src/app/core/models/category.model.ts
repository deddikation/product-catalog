// =============================================================================
// File: category.model.ts
// Layer: Core — Models
// Purpose: TypeScript interfaces for category data models including
//          the hierarchical tree structure.
// =============================================================================

/**
 * Flat category representation for dropdowns and lists.
 * Maps to the backend CategoryDto record.
 */
export interface Category {
  /** Unique category identifier */
  id: number;
  /** Category display name */
  name: string;
  /** Category description */
  description: string;
  /** Parent category ID (null for root categories) */
  parentCategoryId: number | null;
}

/**
 * Hierarchical category tree node.
 * Maps to the backend CategoryTreeDto with custom JSON fields.
 * The 'subcategories' field is used (not 'children') because of our
 * custom CategoryTreeJsonConverter which renames the property.
 */
export interface CategoryTree {
  /** Unique category identifier */
  id: number;
  /** Category display name */
  name: string;
  /** Category description */
  description: string;
  /** Nesting depth (0 = root, added by custom JSON converter) */
  depth: number;
  /** Number of direct children (added by custom JSON converter) */
  childCount: number;
  /** Child category nodes (renamed from 'children' by custom converter) */
  subcategories?: CategoryTree[];
}

/**
 * Payload for creating a new category.
 * Maps to the backend CreateCategoryDto record.
 */
export interface CreateCategory {
  name: string;
  description: string;
  parentCategoryId: number | null;
}
