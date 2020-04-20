using AdventureGrainInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Newtonsoft.Json;

namespace AdventureSetup
{
    public static class Adventure
    {
        public static async Task Configure(IClusterClient client, string filename)
        {
            var rand = new Random();

            using (var jsonStream = new JsonTextReader(File.OpenText(filename)))
            {
                var deserializer = new JsonSerializer();
                var data = deserializer.Deserialize<MapInfo>(jsonStream);

                var rooms = new List<IRoomGrain>();

                // configure rooms

                foreach (var room in data.Rooms)
                {
                    var roomGrain = client.GetGrain<IRoomGrain>(room.Id);
                    await roomGrain.SetInfo(room);
                    if (room.Id >= 0)
                        rooms.Add(roomGrain);
                }

                // configure things

                foreach (var thing in data.Things)
                {
                    var roomGrain = client.GetGrain<IRoomGrain>(thing.FoundIn);
                    await roomGrain.Drop(thing);
                }

                // configure monsters

                foreach (var monster in data.Monsters)
                {
                    var room = rooms[rand.Next(0, rooms.Count)];
                    var monsterGrain = client.GetGrain<IMonsterGrain>(monster.Id);
                    await monsterGrain.SetInfo(monster);
                    await monsterGrain.SetRoomGrain(room);
                }
            }
        }
    }
}
