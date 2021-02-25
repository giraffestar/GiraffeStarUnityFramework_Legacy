using System.Collections;
using GiraffeStar;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class ConfigTest
    {
        [SetUp]
        public void Setup()
        {
            SceneManager.LoadScene("MainTestScene", LoadSceneMode.Single);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator ReadTest()
        {
            yield return new WaitForSceneLoaded("MainTestScene");

            Assert.AreEqual(true, Config.GetBoolOrDefault("BoolTest"));
            Assert.AreEqual(100, Config.GetIntOrDefault("IntTest"));
            Assert.AreEqual(123.456f, Config.GetFloatOrDefault("FloatTest"));
            Assert.AreEqual("Success", Config.GetStringOrDefault("StringTest"));
        }
    }

    public class WaitForSceneLoaded : CustomYieldInstruction
    {
        readonly string sceneName;
        readonly float timeout;
        readonly float startTime;
        bool timedOut;

        public bool TimedOut => timedOut;

        public override bool keepWaiting
        {
            get
            {
                var scene = SceneManager.GetSceneByName(sceneName);
                var sceneLoaded = scene.IsValid() && scene.isLoaded;

                if (Time.realtimeSinceStartup - startTime >= timeout)
                {
                    timedOut = true;
                }

                return !sceneLoaded && !timedOut;
            }
        }

        public WaitForSceneLoaded(string newSceneName, float newTimeout = 10)
        {
            sceneName = newSceneName;
            timeout = newTimeout;
            startTime = Time.realtimeSinceStartup;
        }
    }
}
