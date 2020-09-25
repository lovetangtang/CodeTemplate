CREATE TABLE codesmith_extended_properties
(
    container_object_owner     VARCHAR2(50) NOT NULL,
    object_name                VARCHAR2(61) NOT NULL,
    codesmith_schema_type      VARCHAR2(200) NOT NULL,
    property_name              VARCHAR2(75) NOT NULL,
    property_value             VARCHAR2(4000),
    clr_type                   VARCHAR2(50) NOT NULL
)
/

ALTER TABLE codesmith_extended_properties
    ADD CONSTRAINT pkcodesmithextendedproperties PRIMARY KEY
    (
        container_object_owner,
        object_name,
        codesmith_schema_type,
        property_name
    )
/


