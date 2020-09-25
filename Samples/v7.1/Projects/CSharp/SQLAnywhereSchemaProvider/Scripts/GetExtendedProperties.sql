-- Not used for SQLAnywhere
SELECT
    p.name AS [PropertyName],
    p.value AS [PropertyValue],
    SQL_VARIANT_PROPERTY(p.value,'BaseType') AS [PropertyBaseType],
    SQL_VARIANT_PROPERTY(p.value,'MaxLength') AS [PropertyMaxLength],
    SQL_VARIANT_PROPERTY(p.value,'Precision') AS [PropertyPrecision],
    SQL_VARIANT_PROPERTY(p.value,'Scale') AS [PropertyScale]
FROM
    ::fn_listextendedproperty(NULL, @level0type, @level0name, @level1type, @level1name, @level2type, @level2name) p