USE comfort_furniture;

SELECT 'products' AS table_name, COUNT(*) AS total FROM products
UNION ALL
SELECT 'product_types', COUNT(*) FROM product_types
UNION ALL
SELECT 'material_types', COUNT(*) FROM material_types
UNION ALL
SELECT 'workshops', COUNT(*) FROM workshops
UNION ALL
SELECT 'product_workshops', COUNT(*) FROM product_workshops;

SELECT * FROM v_product_list;