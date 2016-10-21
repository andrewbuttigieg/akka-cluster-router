using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using AkkaClusterRouter.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaClusterRouter
{
    class Program
    {
        static void Main(string[] args)
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

                var someActor = system.ActorOf(Props.Create(() => new SomeActor(12345)), "someActor");
                var props = Props.Create(() => new SomeActor(someActor)).WithRouter(FromConfig.Instance);
                var configNode = ConfigurationFactory.ParseString(nonSeedConfig);

                using (var systemNode = ActorSystem.Create("clusterSystem", configNode))
                {

                    var router = system.ActorOf(props, "talker");

                    system
                       .Scheduler
                       .ScheduleTellRepeatedly(TimeSpan.FromSeconds(0),
                                TimeSpan.FromSeconds(5),
                                router, 654321, ActorRefs.NoSender);

                    Console.ReadLine();
                }
            }
        }
    }
}
