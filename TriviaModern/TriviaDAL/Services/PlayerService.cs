﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using TriviaGame;
using System.Reflection;
using System.Data.SqlClient;
using System.Collections;


namespace TriviaGame
{
    public class PlayerService : IService<Player>
    {
        private DbConnection _conn;
        private DataTable _dataTable = null;
        private Player _player = null;
        private IList<PropertyInfo> _properties = null;


        //Constructor PlayerDAO
        public PlayerService()
        {
            this._conn = new DbConnection();
            this._dataTable = new DataTable();
            this._player = new Player();
            this._properties = DTableExtension.GetPropertiesForType<Player>();
        }

        public Player FindById(int playerId)
        {
            string query = "SELECT * FROM Player WHERE id=@PlayerId";
            SqlParameter[] sqlParameters = new SqlParameter[1];
            sqlParameters[0] = new SqlParameter("@PlayerId", playerId);
            //     return _playerConn.executeSelectQuery<Player>(query, sqlParameters);
            _dataTable = _conn.executeSelectDataTable(query, sqlParameters, CommandType.Text);
            foreach (DataRow _dataRow in _dataTable.Rows)
            {
                _player = DTableExtension.CreateObjFromRow<Player>(_dataRow, _properties);
            }
            return _player;
        }

        public Player Verify(Player login)
        {
            string query = "Select * From Player WHERE email=@email";
            SqlParameter[] sqlParameters = new SqlParameter[1]; //1 params   
            sqlParameters[0] = new SqlParameter("@email", login.email);
            _dataTable = _conn.executeSelectDataTable(query, sqlParameters, CommandType.Text);
            foreach (DataRow _dataRow in _dataTable.Rows)
            {
                _player = DTableExtension.CreateObjFromRow<Player>(_dataRow, _properties);
            }

            if (_player.password != null)
            {
                System.Diagnostics.Debug.WriteLine("^^^ returned pass !!^^^ >>> " + _player.password);
                if (BCrypt.Net.BCrypt.Verify(login.password + "dani", _player.password))
                {
                    System.Diagnostics.Debug.WriteLine("^^^ This User Found and his password OK !! ^^^");
                    return _player;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("^^^Email found but entered Password doesn't match !! ^^^");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public bool Insert(Player player)
        {
            string pwdToHash = player.password + "dani"; // ^Y8~JJ is my hard-coded salt
            string hashToStoreInDatabase = BCrypt.Net.BCrypt.HashPassword(pwdToHash, BCrypt.Net.BCrypt.GenerateSalt());
            string query = "INSERT INTO Player VALUES(@username,@email,@password,@registration_date,@image)";
            SqlParameter[] sqlParameters = new SqlParameter[5];
            sqlParameters[0] = new SqlParameter("@username", player.username);
            sqlParameters[1] = new SqlParameter("@email", player.email);
            sqlParameters[2] = new SqlParameter("@password", hashToStoreInDatabase);
            sqlParameters[3] = new SqlParameter("@registration_date", player.registration_date);
            sqlParameters[4] = new SqlParameter("@image", player.image);

            return _conn.executeQuery(query, sqlParameters,CommandType.Text);

        }

        public bool Update(Player player)
        {
            return false;
        }

        public IList<Player> GetAll()
        {
            List<Player> _players = new List<Player>();
            string query = "SELECT * FROM Player";
            SqlParameter[] sqlParameters = null;
            _dataTable = _conn.executeSelectDataTable(query, sqlParameters, CommandType.Text);
            return DTableExtension.ToAnyList<Player>(_dataTable);
        }

        public bool CheckIfExists(Player player)
        {
            string query = "SELECT * FROM Player p WHERE p.email=@Email";
            SqlParameter[] sqlParameters = new SqlParameter[1];
            sqlParameters[0] = new SqlParameter("@Email", player.email);
            _dataTable = _conn.executeSelectDataTable(query, sqlParameters, CommandType.Text);
            foreach (DataRow _dataRow in _dataTable.Rows)
            {
                _player = DTableExtension.CreateObjFromRow<Player>(_dataRow, _properties);
                if (player.email != null && player.password != null)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Delete(Player entity)
        {
            throw new NotImplementedException();
        }

        public DataTable GetProfileInfo(int userId)
        {
            string _procedureName = "PlayerProfileData";
            SqlParameter[] sqlParameters = new SqlParameter[1];
            sqlParameters[0] = new SqlParameter("@PlayerId", userId);
            return _conn.executeSelectDataTable(_procedureName, sqlParameters, CommandType.StoredProcedure);
        }

        public DataTable GetPlayerAllStats(int userId)
        {
            string _procedureName = "PlayersGamesResult";
            SqlParameter[] sqlParameters = new SqlParameter[1];
            sqlParameters[0] = new SqlParameter("@PlayerId", userId);
            return _conn.executeSelectDataTable(_procedureName, sqlParameters, CommandType.StoredProcedure);
        }

    }
}