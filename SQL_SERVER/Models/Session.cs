namespace SQL_SERVER.Models
{
    public class Session
    {
        private Guid _sessionId;
        private Guid _userId;
        private DateTime _createdAt;
        private DateTime _expiresAt;
        private string _signedSessionId;

        public Guid GetSessionId() => _sessionId;
        public void SetSessionId(Guid sessionId) => _sessionId = sessionId;

        public Guid GetUserId() => _userId;
        public void SetUserId(Guid userId) => _userId = userId;

        public DateTime GetCreatedAt() => _createdAt;
        public void SetCreatedAt(DateTime createdAt) => _createdAt = createdAt;

        public DateTime GetExpiresAt() => _expiresAt;
        public void SetExpiresAt(DateTime expiresAt) => _expiresAt = expiresAt;

        public string GetSignedSessionId() => _signedSessionId;
        public void SetSignedSessionId(string signedSessionId) => _signedSessionId = signedSessionId;
    }
}
