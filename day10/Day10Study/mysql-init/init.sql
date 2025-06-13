CREATE TABLE IF NOT EXISTS products (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(100) NOT NULL,
  price DOUBLE NOT NULL,
  quantity INT NOT NULL
);

INSERT INTO products (name, price, quantity) VALUES
('Mouse', 19.99, 10),
('Keyboard', 49.99, 5);