// =============================================================================
// File: SearchField.cs
// Layer: Domain
// Purpose: Value objects used by the search engine to define searchable fields
//          with their associated weights for relevance scoring.
// =============================================================================

namespace ProductCatalog.Domain.ValueObjects;

/// <summary>
/// Defines a searchable field extracted from an entity, including its name,
/// text value, and relative importance weight for scoring.
/// </summary>
/// <param name="FieldName">Human-readable name of the field (e.g., "Name", "Description").</param>
/// <param name="Value">The text content of this field to search against.</param>
/// <param name="Weight">Relative importance weight (0.0 to 1.0). Higher weight = more influence on final score.</param>
public record SearchField(string FieldName, string Value, double Weight);

/// <summary>
/// Configuration for the search engine specifying how to extract searchable
/// fields from an entity of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The entity type to extract fields from.</typeparam>
/// <param name="FieldExtractor">Function that extracts searchable fields from an entity instance.</param>
public record SearchConfiguration<T>(Func<T, IEnumerable<SearchField>> FieldExtractor) where T : class;
