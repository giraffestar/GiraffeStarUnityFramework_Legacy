using GiraffeStar;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    class MessageTest
    {
        GameSystem gameSystem;

        [SetUp]
        public void Setup()
        {
            gameSystem = new GameSystem();
        }

        [Test]
        public void ModuleMessageTest()
        {
            var testModule = new SubscriberModule();
            gameSystem.RegisterInternal(testModule);
            Assert.AreEqual(0, testModule.AddValue);
            new AddMessage().Dispatch(gameSystem);
            Assert.AreEqual(5, testModule.AddValue);
            gameSystem.UnregisterInternal(testModule);
            new AddMessage().Dispatch(gameSystem);
            Assert.AreEqual(5, testModule.AddValue);
        }

        [Test]
        public void ObjectMessageTest()
        {
            var testObj = new SubscriberObject();
            gameSystem.RegisterInternal(testObj);
            Assert.AreEqual(0, testObj.AddValue);
            new AddMessage().Dispatch(gameSystem);
            Assert.AreEqual(5, testObj.AddValue);
            gameSystem.UnregisterInternal(testObj);
            new AddMessage().Dispatch(gameSystem);
            Assert.AreEqual(5, testObj.AddValue);
        }

        [Test]
        public void UnacceptMultipleParameterTest()
        {
            var testObj = new ExceptionObject();
            Assert.Throws(typeof(FrameworkException), () => { gameSystem.RegisterInternal(testObj); });
        }

        [Test]
        public void ParentMessageTest()
        {
            var testObj = new SubscriberObject();
            gameSystem.RegisterInternal(testObj);
            Assert.AreEqual(0, testObj.SetValue);
            Assert.AreEqual(0, testObj.SetChildValue);
            new SetChildMessage().Dispatch(gameSystem);
            Assert.AreEqual(10, testObj.SetValue);
            Assert.AreEqual(20, testObj.SetChildValue);
            gameSystem.UnregisterInternal(testObj);
        }

        [Test]
        public void ServiceLimitTest()
        {
            var testObj = new DomainObject();
            gameSystem.RegisterInternal(testObj);
            Assert.AreEqual(0, testObj.Value1);
            Assert.AreEqual(0, testObj.Value2);
            Assert.AreEqual(0, testObj.Value3);

            new AddMessage()
            {
                Filter = "DomainA"
            }.Dispatch(gameSystem);
            Assert.AreEqual(5, testObj.Value1);
            Assert.AreEqual(0, testObj.Value2);
            Assert.AreEqual(5, testObj.Value3);

            new AddMessage()
            {
                Filter = "DomainB"
            }.Dispatch(gameSystem);
            Assert.AreEqual(5, testObj.Value1);
            Assert.AreEqual(5, testObj.Value2);
            Assert.AreEqual(10, testObj.Value3);

            gameSystem.UnregisterInternal(testObj);
        }

        [Test]
        public void DispatchTest()
        {
            var testModule = new SubscriberModule();
            var testObj = new SubscriberObject();
            gameSystem.RegisterInternal(testModule);
            gameSystem.RegisterInternal(testObj);
            Assert.AreEqual(0, testModule.SetValue);
            Assert.AreEqual(0, testObj.SetValue);

            new SetMessage().Dispatch(gameSystem);
            Assert.AreEqual(10, testModule.SetValue);
            Assert.AreEqual(10, testObj.SetValue);

            gameSystem.UnregisterInternal(testModule);
            gameSystem.UnregisterInternal(testObj);
        }

        [Test]
        public void UnregisterDuringHandlingTest()
        {
            var insideClass = new InsideClass();
            gameSystem.RegisterInternal(insideClass);
            var unregisterClass = new UnregisterDuringHandle(gameSystem, insideClass);
            gameSystem.RegisterInternal(unregisterClass);
            Assert.AreEqual(false, insideClass.MessageReceived);
            Assert.AreEqual(false, unregisterClass.MessageReceived);

            new UnregisterMessage().Dispatch(gameSystem);
            Assert.AreEqual(true, insideClass.MessageReceived);
            Assert.AreEqual(true, unregisterClass.MessageReceived);
            gameSystem.UnregisterInternal(insideClass);
            gameSystem.UnregisterInternal(unregisterClass);
        }
    }

    class SetMessage : MessageCore
    {
        public int Value = 10;
    }

    class AddMessage : MessageCore
    {
        public int Value = 5;
    }

    class SetChildMessage : SetMessage
    {
        public int ChildValue = 20;
    }

    class SubscriberModule : IModule
    {
        public int SetValue { get; private set; }
        public int AddValue { get; private set; }

        [Subscriber]
        void Handle(SetMessage msg)
        {
            SetValue = msg.Value;
        }

        [Subscriber]
        void Handle(AddMessage msg)
        {
            AddValue += msg.Value;
        }
    }

    class SubscriberObject
    {
        public int SetValue { get; private set; }
        public int SetChildValue { get; private set; }
        public int AddValue { get; private set; }

        [Subscriber]
        void Handle(SetMessage msg)
        {
            SetValue = msg.Value;
        }

        [Subscriber]
        void Handle(SetChildMessage msg)
        {
            SetChildValue = msg.ChildValue;
        }

        [Subscriber]
        void Handle(AddMessage msg)
        {
            AddValue += msg.Value;
        }
    }

    class ExceptionObject
    {
        [Subscriber]
        void HandleToParameter(SetMessage msg1, AddMessage msg2)
        {

        }
    }

    class DomainObject
    {
        public int Value1 { get; private set; }
        public int Value2 { get; private set; }
        public int Value3 { get; private set; }

        [Subscriber("DomainA")]
        void Handle1(AddMessage msg)
        {
            Value1 += msg.Value;
        }

        [Subscriber("DomainB")]
        void Handle2(AddMessage msg)
        {
            Value2 += msg.Value;
        }

        [Subscriber("DomainA", "DomainB")]
        void Handle3(AddMessage msg)
        {
            Value3 += msg.Value;
        }
    }

    class UnregisterMessage : MessageCore
    {

    }

    class UnregisterDuringHandle
    {
        public bool MessageReceived { get; private set; }

        GameSystem gameSystem;
        InsideClass insideClass;

        public UnregisterDuringHandle(GameSystem gameSystem, InsideClass insideClass)
        {
            this.gameSystem = gameSystem;
            this.insideClass = insideClass;
        }

        [Subscriber]
        void Handle(UnregisterMessage msg)
        {
            gameSystem.UnregisterInternal(insideClass);
            MessageReceived = true;
        }
    }

    class InsideClass
    {
        public bool MessageReceived { get; private set; }

        [Subscriber]
        void Handle(UnregisterMessage msg)
        {
            MessageReceived = true;
        }
    }
}