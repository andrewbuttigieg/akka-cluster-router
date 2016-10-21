using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using Akka.TestKit.Xunit2;
using AkkaClusterRouter.Actors;
using System;
using Xunit;

namespace AkkaClusterRouter
{
    public class TestActorRefTest: TestKit
    {
        [Fact]
        public void TestActorPassedInConstructor_ProxiesMessageToTestActor()
        {
             var seedConfig = @"
akka {
    actor {
        provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
        deployment {
            /talker {
                router = broadcast-pool
                routees.paths = [""/user/someActor""]
                nr-of-instances = 5
                virtual-nodes-factor = 10
                cluster {
			        enabled = on
					allow-local-routees = off
	            }
            }
        }
    }
    remote {
        helios.tcp {
            transport-class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
            applied-adapters = []
            transport-protocol = tcp
            port = 50003
            hostname = localhost
        }
    }
    cluster {
        seed-nodes = [""akka.tcp://clusterSystem@localhost:50003""]
    }
}";

 var nonSeedConfig = @"
akka {
    actor {
        provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
        deployment {
        }
    }
    remote {
        helios.tcp {
            transport-class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
            applied-adapters = []
            transport-protocol = tcp
            port = 0
            hostname = localhost
        }
    }
    cluster {
        seed-nodes = [""akka.tcp://clusterSystem@localhost:50003""]
        roles = [""tracker""]
    }
}";

            var config = ConfigurationFactory.ParseString(seedConfig);

            using (var system = ActorSystem.Create("clusterSystem", config))
            {
                Console.WriteLine("Created System.");

                var testActor = CreateTestProbe();
                var props = Props.Create(() => new SomeActor(testActor.Ref)).WithRouter(FromConfig.Instance);
                var configNode = ConfigurationFactory.ParseString(nonSeedConfig);

                using (var systemNode = ActorSystem.Create("clusterSystem", configNode))
                {

                    var router = system.ActorOf(props, "talker");

                    system
                       .Scheduler
                       .ScheduleTellRepeatedly(TimeSpan.FromSeconds(0),
                                TimeSpan.FromSeconds(5),
                                router, 654321, ActorRefs.NoSender);

                    testActor.ExpectMsg<int>();
                }
            }
        }
    }
}
