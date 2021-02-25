using GiraffeStar;
using NUnit.Framework;
using System.Collections;

namespace Tests
{
    class MissionTest
    {
        GameSystem gameSystem;

        [SetUp]
        public void Setup()
        {
            gameSystem = new GameSystem();
        }

        [Test]
        public void BasicTest()
        {
            var missionBox = new MissionBox();
            Assert.AreEqual(0, missionBox.Value);
            missionBox.StartBasicMission(gameSystem);
            Assert.AreEqual(1, missionBox.Value);
            gameSystem.Update();
            Assert.AreEqual(2, missionBox.Value);
        }

        [Test]
        public void FixedUpdateTest()
        {
            var missionBox = new MissionBox();
            Assert.AreEqual(0, missionBox.Value);
            missionBox.StartFixedUpdateMission(gameSystem);
            Assert.AreEqual(1, missionBox.Value);
            gameSystem.FixedUpdate();
            Assert.AreEqual(2, missionBox.Value);
        }

        [Test]
        public void LateUpdateTest()
        {
            var missionBox = new MissionBox();
            Assert.AreEqual(0, missionBox.Value);
            missionBox.StartLateUpdateMission(gameSystem);
            Assert.AreEqual(1, missionBox.Value);
            gameSystem.LateUpdate();
            Assert.AreEqual(2, missionBox.Value);
        }

        [Test]
        public void MessageTest()
        {
            var missionBox = new MissionBox();
            Assert.AreEqual(0, missionBox.Value);
            missionBox.StartMessageMission(gameSystem);
            Assert.AreEqual(1, missionBox.Value);
            new ProgressMessage().Dispatch(gameSystem);
            Assert.AreEqual(2, missionBox.Value);
        }

        [Test]
        public void ReadMessageTest()
        {
            var missionBox = new MissionBox();
            Assert.AreEqual(0, missionBox.Value);
            missionBox.StartReadMessageMission(gameSystem);
            Assert.AreEqual(1, missionBox.Value);
            new ProgressMessage().Dispatch(gameSystem);
            Assert.AreEqual(3, missionBox.Value);
        }

        [Test]
        public void MissionStateTest()
        {
            var mission = new Mission(PauseMission, gameSystem);
            Assert.AreEqual(Mission.MissionState.Ready, mission.State);
            mission.Run();
            Assert.AreEqual(Mission.MissionState.Waiting, mission.State);
            mission.Pause();
            Assert.AreEqual(Mission.MissionState.Paused, mission.State);
            gameSystem.Update();
            Assert.AreEqual(Mission.MissionState.Paused, mission.State);
            mission.Run();
            Assert.AreEqual(Mission.MissionState.Waiting, mission.State);
            gameSystem.Update();
            Assert.AreEqual(Mission.MissionState.Completed, mission.State);

            mission.Reset();
            Assert.AreEqual(Mission.MissionState.Ready, mission.State);
            mission.Run();
            Assert.AreEqual(Mission.MissionState.Waiting, mission.State);
            mission.Cancel();
            Assert.AreEqual(Mission.MissionState.Cancelled, mission.State);
            gameSystem.Update();
            Assert.AreEqual(Mission.MissionState.Cancelled, mission.State);

            mission.Reset();
            Assert.AreEqual(Mission.MissionState.Ready, mission.State);
        }

        IEnumerator PauseMission()
        {
            yield return new WaitFor.Update();
        }
    }

    class ProgressMessage : MessageCore
    {
        public int Value = 3;
    }

    class MissionBox
    {
        public int Value { get; private set; }

        public void StartBasicMission(GameSystem gameSystem)
        {
            var mission = new Mission(BasicMission, gameSystem);
            mission.Run();
        }

        public void StartLateUpdateMission(GameSystem gameSystem)
        {
            var mission = new Mission(LateUpdateMission, gameSystem);
            mission.Run();
        }

        public void StartFixedUpdateMission(GameSystem gameSystem)
        {
            var mission = new Mission(FixedUpdateMission, gameSystem);
            mission.Run();
        }

        public void StartMessageMission(GameSystem gameSystem)
        {
            var mission = new Mission(MessageMission, gameSystem);
            mission.Run();
        }

        public void StartReadMessageMission(GameSystem gameSystem)
        {
            var mission = new Mission(ReadMessageMission, gameSystem);
            mission.Run();
        }

        IEnumerator BasicMission()
        {
            Value = 1;
            yield return new WaitFor.Update();
            Value = 2;
        }

        IEnumerator LateUpdateMission()
        {
            Value = 1;
            yield return new WaitFor.LateUpdate();
            Value = 2;
        }

        IEnumerator FixedUpdateMission()
        {
            Value = 1;
            yield return new WaitFor.FixedUpdate();
            Value = 2;
        }

        IEnumerator MessageMission()
        {
            Value = 1;
            yield return new WaitFor.Message<ProgressMessage>();
            Value = 2;
        }

        IEnumerator ReadMessageMission()
        {
            Value = 1;
            var msg = new WaitFor.Message<ProgressMessage>();
            yield return msg;
            Value = msg.Msg.Value;
        }
    }
}