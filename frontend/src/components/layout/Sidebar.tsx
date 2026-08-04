import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Search, MessageSquare, Trash2, LogOut, Plus, FileText } from "lucide-react";
import { getConversations, createConversation, deleteConversation } from "../../api/chat";
import { authStore, useAuth } from "../../store/authStore";
import type { Conversation } from "../../types";
import DocumentsModal from "../documents/DocumentsModal";

interface Props {
  activeId: string | null;
  onSelect: (id: string, title?: string) => void;
}

function formatTime(dateStr: string) {
  const d = new Date(dateStr);
  const now = new Date();
  const isToday = d.toDateString() === now.toDateString();
  return isToday
    ? d.toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" })
    : d.toLocaleDateString("tr-TR", { day: "2-digit", month: "2-digit" });
}

export default function Sidebar({ activeId, onSelect }: Props) {
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [search, setSearch] = useState("");
  const [showDocuments, setShowDocuments] = useState(false);
  const { user } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    authStore.logout();
    navigate("/login", { replace: true });
  };

  useEffect(() => {
    getConversations().then((r) => setConversations(r.data));
  }, []);

  const handleNew = async () => {
    const { data } = await createConversation();
    setConversations((prev) => [data, ...prev]);
    onSelect(data.id, data.title);
  };

  const handleDelete = async (e: React.MouseEvent, id: string) => {
    e.stopPropagation();
    await deleteConversation(id);
    setConversations((prev) => prev.filter((c) => c.id !== id));
    if (activeId === id) onSelect("");
  };

  const filtered = conversations.filter((c) =>
    (c.title || "Yeni Sohbet").toLowerCase().includes(search.toLowerCase())
  );

  const userInitial = user?.email?.[0]?.toUpperCase() ?? "U";

  return (
    <aside className="sidebar">
      <div className="sidebar-top">
        <button className="sidebar-new-chat-btn" onClick={handleNew}>
          <Plus size={17} />
          Yeni Sohbet
        </button>
      </div>

      {showDocuments && <DocumentsModal onClose={() => setShowDocuments(false)} />}

      <div className="sidebar-search">
        <div className="sidebar-search-input-wrap">
          <Search size={15} />
          <input
            className="sidebar-search-input"
            placeholder="Sohbet ara..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </div>

      <nav className="sidebar-nav">
        {filtered.length === 0 && (
          <div style={{ padding: "24px 16px", textAlign: "center", color: "var(--text-muted)", fontSize: 13 }}>
            {search ? "Sohbet bulunamadı" : "Henüz sohbet yok"}
          </div>
        )}
        {filtered.map((c) => (
          <div
            key={c.id}
            className={`sidebar-item ${activeId === c.id ? "active" : ""}`}
            onClick={() => onSelect(c.id, c.title)}
          >
            <span className="conv-icon">
              <MessageSquare size={16} />
            </span>
            <div className="conv-info">
              <span className="conv-name">{c.title || "Yeni Sohbet"}</span>
              <span className="conv-time">{formatTime(c.updatedAt)}</span>
            </div>
            <button
              className="conv-delete"
              onClick={(e) => handleDelete(e, c.id)}
              title="Sohbeti Sil"
            >
              <Trash2 size={15} />
            </button>
          </div>
        ))}
      </nav>

      <div className="sidebar-footer">
        <div className="sidebar-user-row">
          <div className="sidebar-avatar">{userInitial}</div>
          <span className="sidebar-user-email">{user?.email}</span>
        </div>
        <div className="sidebar-header-icons">
          <button className="btn-icon" onClick={() => setShowDocuments(true)} title="Dokümanlar">
            <FileText size={18} />
          </button>
          <button className="btn-icon" onClick={handleLogout} title="Çıkış Yap">
            <LogOut size={18} />
          </button>
        </div>
      </div>
    </aside>
  );
}
