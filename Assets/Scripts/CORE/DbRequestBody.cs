using Mono.Data.Sqlite;
using septim.core;
using System.Data;
using Unity.Jobs;
using UnityEngine;
using System.Collections;
using System.Threading;

public class DbRequestBody : MonoBehaviour
{ 
    // Start is called before the first frame update
    //public IDbConnection connection;
    //

    string db;
    string commandLine;
    JobHandle job;

    public void init(string db, string commandLine)
    {
        this.db = db;
        this.commandLine = commandLine;
        job = startRequest();
        KillJob();
    }

    private JobHandle startRequest()
    {
        JobBody node = new JobBody(db, commandLine);
        return node.Schedule();
    }

    public void KillJob()
    {
        IEnumerator coroutine = _KillJob();
        StartCoroutine(coroutine);
    }

    private IEnumerator _KillJob()
    {
        yield return new WaitForSeconds(60);
        job.Complete();
        Destroy(this.gameObject);
    }
}

public struct JobBody : IJob
{
    string db;
    string commandLine;
    public JobBody(string db, string commandLine)
    {
        this.db = db;
        this.commandLine = commandLine;
    }

    public void Execute()
    {
        IDbConnection connection = new SqliteConnection(db);
        IDbCommand command = connection.CreateCommand();
        command.CommandText = commandLine;
        using (IDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {

            }
            reader.Close();
        }
        connection.Close();
    }
}
