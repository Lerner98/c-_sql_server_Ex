using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_SERVER.Models
{
    public class AuditLog
    {
        private Guid _logId;
        private Guid _userId;
        private string _action;
        private string _tableName;
        private Guid _recordId;
        private DateTime _actionDate;
        private string _details;

        public Guid GetLogId()
        {
            return this._logId;
        }

        public void SetLogId(Guid logId)
        {
            this._logId = logId;
        }

        public Guid GetUserId()
        {
            return this._userId;
        }

        public void SetUserId(Guid userId)
        {
            this._userId = userId;
        }

        public string GetAction()
        {
            return this._action;
        }

        public void SetAction(string action)
        {
            this._action = action;
        }

        public string GetTableName()
        {
            return this._tableName;
        }

        public void SetTableName(string tableName)
        {
            this._tableName = tableName;
        }

        public Guid GetRecordId()
        {
            return this._recordId;
        }

        public void SetRecordId(Guid recordId)
        {
            this._recordId = recordId;
        }

        public DateTime GetActionDate()
        {
            return this._actionDate;
        }

        public void SetActionDate(DateTime actionDate)
        {
            this._actionDate = actionDate;
        }

        public string GetDetails()
        {
            return this._details;
        }

        public void SetDetails(string details)
        {
            this._details = details;
        }
    }
}
