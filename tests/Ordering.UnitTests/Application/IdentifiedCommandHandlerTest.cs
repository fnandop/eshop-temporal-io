using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Infrastructure.Repositories;

namespace eShop.Ordering.UnitTests.Application;

[TestClass]
public class IdentifiedCommandHandlerTest
{
    private readonly IRequestManager _requestManager;
    private readonly IMediator _mediator;
    private readonly ILogger<IdentifiedCommandHandler<CreateOrderCommand, int>> _loggerMock;
    private readonly IOrderRepository _orderRepository;

    public IdentifiedCommandHandlerTest()
    {
        _requestManager = Substitute.For<IRequestManager>();
        _mediator = Substitute.For<IMediator>();
        _loggerMock = Substitute.For<ILogger<IdentifiedCommandHandler<CreateOrderCommand, int>>>();
        _orderRepository = Substitute.For<IOrderRepository>(); ;
    }

    [TestMethod]
    public async Task Handler_sends_command_when_order_no_exists()
    {
        // Arrange
        var fakeGuid = Guid.NewGuid();
        var fakeOrderCmd = new IdentifiedCommand<CreateOrderCommand, int>(FakeOrderRequest(), fakeGuid);

        _requestManager.ExistAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(false));

        _mediator.Send(Arg.Any<IRequest<int>>(), default)
            .Returns(Task.FromResult(999));

        // Act
        var handler = new CreateOrderIdentifiedCommandHandler(_mediator, _requestManager, _loggerMock, _orderRepository);
        var result = await handler.Handle(fakeOrderCmd, CancellationToken.None);

        // Assert
        Assert.AreEqual(999, result);
        await _mediator.Received().Send(Arg.Any<IRequest<int>>(), default);
    }

    [TestMethod]
    public async Task Handler_sends_no_command_when_order_already_exists()
    {
        // Arrange
        var fakeGuid = Guid.NewGuid();
        var fakeOrderCmd = new IdentifiedCommand<CreateOrderCommand, int>(FakeOrderRequest(), fakeGuid);

        _requestManager.ExistAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(true));

        _mediator.Send(Arg.Any<IRequest<int>>(), default)
            .Returns(Task.FromResult(999));


        var street = "fakeStreet";
        var city = "FakeCity";
        var state = "fakeState";
        var country = "fakeCountry";
        var zipcode = "FakeZipCode";
        var cardTypeId = 5;
        var cardNumber = "12";
        var cardSecurityNumber = "123";
        var cardHolderName = "FakeName";
        var cardExpiration = DateTime.UtcNow.AddYears(1);
        var fakeOrder = new Order("b66b20c7-5d40-40c5-b701-982e092bc22f", "1", "fakeName", new Address(street, city, state, country, zipcode), cardTypeId, cardNumber, cardSecurityNumber, cardHolderName, cardExpiration);

        _orderRepository.GetAsyncByOrderGuid(Arg.Any<string>())
            .Returns(Task.FromResult(fakeOrder));

        // Act
        var handler = new CreateOrderIdentifiedCommandHandler(_mediator, _requestManager, _loggerMock, _orderRepository);
        var result = await handler.Handle(fakeOrderCmd, CancellationToken.None);

        // Assert
        await _mediator.DidNotReceive().Send(Arg.Any<IRequest<int>>(), default);
    }

    private CreateOrderCommand FakeOrderRequest(Dictionary<string, object> args = null)
    {
        return new CreateOrderCommand(
            orderyGuid: args != null && args.ContainsKey("orderyGuid") ? (string)args["orderyGuid"] : null,
            new List<BasketItem>(),
            userId: args != null && args.ContainsKey("userId") ? (string)args["userId"] : null,
            userName: args != null && args.ContainsKey("userName") ? (string)args["userName"] : null,
            city: args != null && args.ContainsKey("city") ? (string)args["city"] : null,
            street: args != null && args.ContainsKey("street") ? (string)args["street"] : null,
            state: args != null && args.ContainsKey("state") ? (string)args["state"] : null,
            country: args != null && args.ContainsKey("country") ? (string)args["country"] : null,
            zipcode: args != null && args.ContainsKey("zipcode") ? (string)args["zipcode"] : null,
            cardNumber: args != null && args.ContainsKey("cardNumber") ? (string)args["cardNumber"] : "1234",
            cardExpiration: args != null && args.ContainsKey("cardExpiration") ? (DateTime)args["cardExpiration"] : DateTime.MinValue,
            cardSecurityNumber: args != null && args.ContainsKey("cardSecurityNumber") ? (string)args["cardSecurityNumber"] : "123",
            cardHolderName: args != null && args.ContainsKey("cardHolderName") ? (string)args["cardHolderName"] : "XXX",
            cardTypeId: args != null && args.ContainsKey("cardTypeId") ? (int)args["cardTypeId"] : 0);
    }
}
