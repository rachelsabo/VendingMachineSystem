using VendingMachine.Domain.Entities;
using VendingMachine.Domain.Exceptions;

namespace VendingMachine.Tests;

public class VendingMachineDomainTests
{
    [Fact]
    public void Cannot_Exceed_Max_Shelves()
    {
        // Arrange
        var vm = new VendingMachine.Domain.Entities.VendingMachine(Guid.NewGuid(), "Test VM", maxShelves: 1);
        var categoryId = Guid.NewGuid();

        vm.AddShelf(categoryId, capacity: 10);

        // Act & Assert
        Assert.Throws<MaxShelvesReachedException>(() => vm.AddShelf(categoryId, capacity: 5));
    }

    [Fact]
    public void Cannot_Load_Wrong_Category()
    {
        // Arrange
        var correctCategoryId = Guid.NewGuid();
        var wrongCategoryId = Guid.NewGuid();
        var vm = new VendingMachine.Domain.Entities.VendingMachine(Guid.NewGuid(), "Test VM", maxShelves: 2);
        var shelf = vm.AddShelf(correctCategoryId, capacity: 10);

        var product = new Product(Guid.NewGuid(), "Soda", wrongCategoryId);

        // Act & Assert
        Assert.Throws<InvalidProductCategoryException>(() => vm.LoadInventory(shelf.Id, product, quantity: 1));
    }

    [Fact]
    public void Cannot_Exceed_Shelf_Capacity()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var vm = new VendingMachine.Domain.Entities.VendingMachine(Guid.NewGuid(), "Test VM", maxShelves: 1);
        var shelf = vm.AddShelf(categoryId, capacity: 5);
        var product = new Product(Guid.NewGuid(), "Soda", categoryId);

        // Act
        vm.LoadInventory(shelf.Id, product, quantity: 5);

        // Assert
        Assert.Throws<ShelfCapacityExceededException>(() => vm.LoadInventory(shelf.Id, product, quantity: 1));
    }

    [Fact]
    public void Successful_Purchase_Reduces_Inventory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var vm = new VendingMachine.Domain.Entities.VendingMachine(Guid.NewGuid(), "Test VM", maxShelves: 1);
        var shelf = vm.AddShelf(categoryId, capacity: 5);
        var productId = Guid.NewGuid();
        var product = new Product(productId, "Soda", categoryId);

        vm.LoadInventory(shelf.Id, product, quantity: 2);

        // Act
        vm.Purchase(productId);

        // Assert
        Assert.Equal(1, shelf.Inventory);
    }

    [Fact]
    public void Purchase_Fails_When_Out_Of_Stock()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var vm = new VendingMachine.Domain.Entities.VendingMachine(Guid.NewGuid(), "Test VM", maxShelves: 1);
        var shelf = vm.AddShelf(categoryId, capacity: 5);
        var productId = Guid.NewGuid();
        var product = new Product(productId, "Soda", categoryId);

        vm.LoadInventory(shelf.Id, product, quantity: 1);

        vm.Purchase(productId);

        // Act & Assert
        Assert.Throws<OutOfStockException>(() => vm.Purchase(productId));
    }

    [Fact]
    public void EndToEnd_Scenarios_1_To_6()
    {
        // Scenario 1: Create vending machine that supports up to 5 shelves
        var vm = new VendingMachine.Domain.Entities.VendingMachine(Guid.NewGuid(), "Scenario VM", maxShelves: 5);

        var drinksCategoryId = Guid.NewGuid();
        var snacksCategoryId = Guid.NewGuid();

        // Scenario 2: Add 3 shelves (10 drinks, 20 snacks, 15 cans drinks)
        var drinksShelf1 = vm.AddShelf(drinksCategoryId, capacity: 10);
        var snacksShelf = vm.AddShelf(snacksCategoryId, capacity: 20);
        var drinksShelf2 = vm.AddShelf(drinksCategoryId, capacity: 15);

        // Scenario 3: Load 5 bottles of water on first drinks shelf
        var waterProductId = Guid.NewGuid();
        var waterProduct = new Product(waterProductId, "Water Bottle", drinksCategoryId);
        vm.LoadInventory(drinksShelf1.Id, waterProduct, quantity: 5);
        Assert.Equal(5, drinksShelf1.Inventory);

        // Scenario 4: Try to load chips (snack) on drinks shelf -> should fail
        var chipsProduct = new Product(Guid.NewGuid(), "Chips", snacksCategoryId);
        Assert.Throws<InvalidProductCategoryException>(() =>
            vm.LoadInventory(drinksShelf1.Id, chipsProduct, quantity: 1));

        // Scenario 5: Try to add a sixth shelf -> should fail
        vm.AddShelf(drinksCategoryId, capacity: 5);  // 4th shelf
        vm.AddShelf(drinksCategoryId, capacity: 5);  // 5th shelf
        Assert.Throws<MaxShelvesReachedException>(() =>
            vm.AddShelf(drinksCategoryId, capacity: 5)); // 6th shelf

        // Scenario 6: Load 18 bottles on shelf with capacity 10 -> should fail
        var vm2 = new VendingMachine.Domain.Entities.VendingMachine(Guid.NewGuid(), "Scenario VM 2", maxShelves: 1);
        var capacity10Shelf = vm2.AddShelf(drinksCategoryId, capacity: 10);
        Assert.Throws<ShelfCapacityExceededException>(() =>
            vm2.LoadInventory(capacity10Shelf.Id, waterProduct, quantity: 18));
    }
}