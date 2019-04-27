using NinjaDomain.Classes;
using NinjaDomain.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.SetInitializer(new NullDatabaseInitializer<NinjaContext>());
            // InsertNinja();
            // InsertMultipleNinjas();
            // SimpleNinjaQueries();
            // QueryAndUpdateNinja();
            // QueryAndUpdateNinjaDisconnected();
            // RetrieveDataWithFind();
            // RetrieveDataWithStoredProc();
            // DeleteNinja();
            // InsertNinjaWithEquipment();
            SimpleNinjaGraphQuery();
            Console.ReadKey();
        }

        private static void InsertNinja()
        {
            var ninja = new Ninja
            {
                Name = "JohnDoeSan",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(1960, 1, 1),
                ClanId = 1
            };

            // The DBContext has DbSets. These DbSets give us access to each set of types
            using (var context = new NinjaContext())
            {
                // we want to work with a set of ninja types
                // Using Add method tells context this a new ninja to insert
                context.Database.Log = Console.WriteLine;
                context.Ninjas.Add(ninja);
                context.SaveChanges();
            }
        }

        private static void InsertMultipleNinjas()
        {
            var ninja1 = new Ninja
            {
                Name = "Leonardo",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(1984, 1, 1),
                ClanId = 1,
            };

            var ninja2 = new Ninja
            {
                Name = "Raphael",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(1985, 1, 1),
                ClanId = 1,
            };

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                context.Ninjas.AddRange(new List<Ninja> { ninja1, ninja2 });
                context.SaveChanges();
            }
        }

        private static void SimpleNinjaQueries()
        {
            // Like the insert, the queries are expressed against 
            // a db set.
            using (var context = new NinjaContext())
            {
                // Select * ninjas in Ninjas table
                var ninjasQuery = context.Ninjas.ToList();
                var peterQuery = context.Ninjas
                    .Where(nj => nj.Name == "PeterSan")
                    .FirstOrDefault();
                foreach (var ninja in ninjasQuery)
                {
                    Console.WriteLine($"Ninja: {ninja.Name}");
                }
                Console.WriteLine($"Ninja: {peterQuery.Name}");
            }
        }

        private static void QueryAndUpdateNinja()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninja = context.Ninjas.FirstOrDefault();
                ninja.ServedInOniwaban = (!ninja.ServedInOniwaban);
                context.SaveChanges();
            }
        }

        private static void QueryAndUpdateNinjaDisconnected()
        {
            Ninja ninja;
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                ninja = context.Ninjas.FirstOrDefault();
            }

            ninja.ServedInOniwaban = (!ninja.ServedInOniwaban);

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                // this says hey pay attention to this in-memory ninja
                // but this is a new context, so it doesn't know that this isn't 
                // a new ninja (event though the ninja has an ID). Instead we have to manually inform the context of this
                // Ninja's state.
                context.Ninjas.Attach(ninja);
                // Instead we manually identify a modified state instead of EF tracking it
                // since this is a different context.
                context.Entry(ninja).State = EntityState.Modified; // EntityState enum
                // The resulting SQL query with UPDATE and SET Every column in the associated table
                // for this ninja instance since it only knows the record is updated but not 
                // which field.
                context.SaveChanges();

            }
        }

        private static void RetrieveDataWithFind()
        {
            var keyval = 4;
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                // Very common that you have the key for a particular object you need from db
                // Find will Select for that object IF the DB context is not already "tracking"
                // that object and has it in memory. It doesn't "waste a trip to the db" if it already has it.
                var ninja = context.Ninjas.Find(keyval);
                Console.WriteLine("After Find#1: " + ninja.Name);

                var someNinja = context.Ninjas.Find(keyval);
                Console.WriteLine("After Find#2 " + someNinja.Name);
                ninja = null;
            }
        }

        private static void RetrieveDataWithStoredProc()
        {
            using (var context = new NinjaContext())
            {
                // A stored procedure is basically a "stored" function in SQL server.
                // In SQL, you can create a "Procedure" and the execute it.
                context.Database.Log = Console.WriteLine;
                var ninjas = context.Ninjas.SqlQuery("exec GetOldNinjas");
                foreach (var ninja in ninjas)
                {
                    Console.WriteLine(ninja.Name);
                }
            }
        }

        private static void DeleteNinja()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninja = context.Ninjas.FirstOrDefault();
                context.Ninjas.Remove(ninja);
                context.SaveChanges();
            }
        }

        private static void DeleteNinjaDisconnected()
        {
            // This mimicks how you might delete an object from the db
            // in a real app, since the act of retrieving the instance object
            // will be "disassociated" from the act of deleting it.
            Ninja ninja;
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                ninja = context.Ninjas.FirstOrDefault();
            }

            // New context that we need to tell about the ninja
            // in order to then delete it
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                // One way:
                // context.Ninjas.Attach(ninja); // use keyval to "attach" in-memory ninja object to context instance
                // context.Ninjas.Remove(ninja);
                // Another way that is more common:
                context.Entry(ninja).State = EntityState.Deleted;
                // It's better than finding a ninja by ID using ".Find()" and then ".Remove()"
                // because the way above requires only one trip to the DB.
                context.SaveChanges();
            }
        }

        private static void InsertNinjaWithEquipment()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                var ninja = new Ninja
                {
                    Name = "Kacy Catanzaro",
                    ServedInOniwaban = false,
                    DateOfBirth = new DateTime(1990, 1, 14),
                    ClanId = 1
                };

                var muscles = new NinjaEquipment
                {
                    Name = "Muscles",
                    Type = EquipmentType.Tool
                };

                var spunk = new NinjaEquipment
                {
                    Name = "Spunk",
                    Type = EquipmentType.Weapon
                };

                // Because EF is already tracking the ninja, it will pick up on the 
                // relationship between the ninja and its equipment even though we didn't
                // explicitly tell the context about the equipment.
                context.Ninjas.Add(ninja);
                ninja.EquipmentOwned.Add(muscles);
                ninja.EquipmentOwned.Add(spunk);
                context.SaveChanges();
            }
        }

        private static void SimpleNinjaGraphQuery()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                // Use DbSet.Include to auto join related data in another table

                var ninja = context.Ninjas
                    .Include(n => n.EquipmentOwned)
                    .FirstOrDefault(n => n.Name.StartsWith("Kacy"));
            }
        }
    }
}
