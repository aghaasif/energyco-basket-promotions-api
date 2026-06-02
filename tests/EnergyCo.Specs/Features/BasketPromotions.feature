Feature: Basket promotions

Scenario: Calculate discounts and loyalty points for a basket
  Given a customer has a basket with eligible fuel and shop products
  When the basket promotions are calculated
  Then the response includes the calculated promotion outcome
