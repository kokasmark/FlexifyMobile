using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace FlexifyMobile
{
    public class FlexifyDatabase
    {
        SQLiteAsyncConnection Database;
        public FlexifyDatabase()
        {

        }
        async Task Init()
        {
            if (Database is not null)
                return;
            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            await Database.CreateTableAsync<User>();
            await Database.CreateTableAsync<WorkoutTemplate>();
        }

        public List<User> GetItemsAsync()
        {
            Init();
            return Database.Table<User>().ToListAsync().Result;
        }

        public async Task<int> SaveItemAsync(User User)
        {
            await Init();
            return await Database.InsertAsync(User);
        }

        public async Task<int> DeleteItemAsync(User User)
        {
            await Init();
            return await Database.DeleteAsync(User);
        }

        public List<WorkoutTemplate> GetWorkoutsAsync()
        {
            Init();
            return Database.Table<WorkoutTemplate>().ToListAsync().Result;
        }

        public async Task<int> SaveWorkoutsAsync(WorkoutTemplate w)
        {
            await Init();
            return await Database.InsertAsync(w);
        }

        public async Task<int> DeleteWorkoutsAsync(WorkoutTemplate w)
        {
            await Init();
            return await Database.DeleteAsync(w);
        }
    }
    
}
