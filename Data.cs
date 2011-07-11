using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Prelude;

namespace nthings2.src.app
{
    public interface IConnectionFactory<T> where T : IDbConnection
    {
        T GetConnection(string connectionString);
    }

    public class SqlConnectionFactory : IConnectionFactory<SqlConnection>
    {
        public SqlConnection GetConnection(string connectionString)
        {
            return new SqlConnection (connectionString);
        }
    }
    
    public abstract class DataContext<ConnectionFactory, Connection> : IEnumerable<IDataReader>, IDisposable where ConnectionFactory : IConnectionFactory<Connection>, new() where Connection : class, IDbConnection
    {
        static string connection = ConfigurationManager.ConnectionStrings["default"].ConnectionString;
        protected string sql;
        
        Action dispose;

        protected List<IDbDataParameter> _params = new List<IDbDataParameter> ();

        public void Add(IDbDataParameter param)
        {
            _params.Add (param);
        }

        public void Add(IEnumerable<IDbDataParameter > @params)
        {
            this._params.AddRange (@params);
        }

        public void Remove(IDbDataParameter param)
        {
            _params.Remove (param);
        }


        public IEnumerator<IDataReader> GetEnumerator()
        {            
            var command = GetCommand();

            foreach (var p in _params)
                command.Parameters.Add (p);
            
            var rdr = command.ExecuteReader ();

            while (rdr.Read ())
                yield return rdr;
        }

        public virtual IDbCommand GetCommand(Connection conn = null)
        {
            conn = conn ?? new ConnectionFactory ().GetConnection (connection);
            conn.Open ();

            dispose += () => { conn.Dispose (); };

            var command = conn.CreateCommand ();

            command.CommandText = sql;

            foreach (var param in _params)
                command.Parameters.Add (param);
            
            command.CommandType = 
                
                sql.StartsWith("[") || sql.EndsWith("]") ||
                
                !sql.Any (x => char.IsWhiteSpace(x) )  ? CommandType.StoredProcedure : CommandType.Text;

            return command;
        }


        public void ExecuteNonQuery()
        {
            var command = GetCommand ();
            (command as SqlCommand).ExecuteNonQuery ();
                
        }

        

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator ();
        }

        public void Dispose()
        {
            if (dispose != null)
                dispose ();
        }
    }


    public abstract class DbContext : DataContext<SqlConnectionFactory, SqlConnection>
    {
        public DbContext(string sql)
        {
            this.sql = sql;
        }
    }   


    public class AddMemberContext : DbContext
    {
        SqlParameter member_id   = new SqlParameter ("@member_id", null ) { Direction = ParameterDirection.Output, Size = 4 };
        SqlParameter session     = new SqlParameter ("@session", null) { Direction = ParameterDirection.Output, Size = 128 };

        MemberSession _member;
        
        public AddMemberContext(string email, string pass, string name, DateTime dob, byte sex, decimal gender ) : base("AddMember")
        {
            this._params.Add (new SqlParameter ("@email", email));
            this._params.Add (new SqlParameter ("@password", pass));
            this._params.Add (new SqlParameter ("@name", name));
            this._params.Add (new SqlParameter ("@dob", dob));
            this._params.Add (new SqlParameter ("@sex", sex));
            this._params.Add (new SqlParameter ("@gender", gender));

            this._params.Add (member_id);
            this._params.Add(session);
        }

        public MemberSession Execute()
        {
            if (_member == null)
            {
                this.ExecuteNonQuery ();

                var member = (-1).Becomes (member_id.Value.ToString());
                var session_id = Guid.Parse ("{00000000-0000-0000-0000-000000000000}").Becomes (session.Value.ToString ());

                _member = new MemberSession (member, session_id, member.SuccessfulParse && session_id.SuccessfulParse);

                this.Dispose ();
            }

            return _member;
        }

        public class MemberSession
        {
            public readonly int MemberId;
            public readonly Guid Session;
            public readonly bool Valid;

            public MemberSession(int x, Guid y, bool valid)
            {
                this.MemberId = x;
                this.Session = y;
                this.Valid = valid;
            }
        }
    }

    public abstract class ListFetcherBase : DbContext
    {
        int list_id;
        string sql;

        protected ListFetcherBase(string sql, int list_id) : base(sql)
        {
            this.list_id = list_id;
        }
        
        public ListThing Execute()
        {
            var xs = new ListThing (null, list_id);

            var rdr = this.GetCommand ().ExecuteReader ();

            int n = 0;
            
            while (rdr.Read ())
            {
                n++;
                
                if (xs.Title == null)
                    xs.Title = rdr["title"].ToString ();

                if (!rdr.IsDBNull (rdr.GetOrdinal ("item_id")))
                {

                    var content = rdr["content"].ToString ();                    
                    var item_id = rdr.GetInt32 (rdr.GetOrdinal ("item_id"));
                    
                    xs.Items.Add (new ListItem (content, xs, item_id) );
                }
            }

            if (n == 0)
                throw new NotFoundException ("no records");

            return xs;
        }

    }


    public class ListFetcher : ListFetcherBase
    {
        public ListFetcher(int id)
            : base ("select * from ListWithItems where list_id = " + id, id)
        {
            
        }

       
    }

    public class ListItemFetcher : ListFetcherBase
    {
        int item_id;

        public ListItemFetcher(int item_id, int list_id)
            : base ("select * from ListWithItems where list_id = " + list_id + " and item_id =" + item_id, list_id)
        {
            this.item_id = item_id;
        }
    }

    
    public abstract class ObjectBase
    {
        public int ID { get; protected set; }
        public DateTime Created { get; protected set; }
        public int Owner { get; protected set; }
    }

    public class ListThing : ObjectBase, IEnumerable<ListItem>
    {
        public string Title { get; set; }
        public List<ListItem> Items { get; protected set; }

        public ListThing(string Title, int ID)
        {
            this.Title = Title;
            this.ID = ID;
            this.Items = new List<ListItem> ();
        }

        public IEnumerator<ListItem> GetEnumerator()
        {
            return Items.GetEnumerator ();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator ();
        }
    }

    public class ListItem : ObjectBase
    {
        public string Content { get; set; }
        public ListThing Parent { get; protected set; }
        public int ItemId { get; protected set; }

        public ListItem(string Content, ListThing Parent, int ItemId)
        {
            this.Content = Content;
            this.Parent = Parent;
            this.ItemId = ItemId;
        }
    }

    public class SaveListItemAction : DbContext
    {
        SqlConnection conn;

        public SaveListItemAction(int id, int creator, int requestor, int? inspiration, int list_id, string content, SqlConnection conn = null)
            : base ("SaveListItem")
        {
            this._params.Add (new SqlParameter ("@id", id));
            this._params.Add (new SqlParameter ("@creator", creator));
            this._params.Add (new SqlParameter ("@list_id", list_id));
            this._params.Add (new SqlParameter ("@content", content));
            this._params.Add (new SqlParameter ("@requestor", requestor));

            if(inspiration.HasValue)
                this._params.Add (new SqlParameter ("@inspiration", inspiration));

            this.conn = conn;
        }

        public int Execute()
        {
            if (conn != null && conn.State != ConnectionState.Open)
                conn.Open ();

            return GetCommand (conn).ExecuteNonQuery ();
        }
    }


    public class DeleteListItemAction : DbContext
    {
        public DeleteListItemAction(int listItem, int requestor)
            : this (new[] { listItem } , requestor) { }

        public DeleteListItemAction(IEnumerable<int> ids, int requestor)
            : this (ids.Select ((x, n) => new SqlParameter ("@id" + n, x)), requestor) { }

        public DeleteListItemAction(IEnumerable<SqlParameter> ids, int requestor)
            : base ("delete from listitem where id in (" + string.Join (",", ids.Select (x => x.ParameterName)) + ")")
        {
            this._params.AddRange (ids);
        }

        public int Execute()
        {
            return this.GetCommand ().ExecuteNonQuery ();
        }
    }

    public class MakeExplicationRequest : DbContext, Controller
    {

        Controller controller;
        int item;
        int list;
        
        public MakeExplicationRequest(int requestor, string text, int itemID, int listID, Controller parent, int? id = null)
            : base ("MakeExplicationRequest")
        {
            this._params.Add (new SqlParameter ("@id", id) { Direction = ParameterDirection.Output, Size = 4 });
            this._params.Add (new SqlParameter ("@requestor", requestor));
            this._params.Add (new SqlParameter ("@request_text", text));
            this._params.Add (new SqlParameter ("@inspiration", itemID));

            this.controller = parent;

            this.item = itemID;
            this.list = listID;
        }

        public void Execute()
        {
            this.GetCommand ().ExecuteNonQuery ();
        }

        public Controller HandleMessage(string message, HttpContextBase context)
        {
            return controller.HandleMessage (message, context);
        }

        public void ProcessRequest(HttpContext context)
        {
            this.Execute ();
            
            context.Response.Redirect ("/list/" + list);
        }

        public bool IsReusable { get { return false; } }

    }


    public abstract class Action<T>
    {
        public abstract T Execute();
    }
}