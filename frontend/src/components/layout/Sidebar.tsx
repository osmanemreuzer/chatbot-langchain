import { useEffect, useState } from "react";
import { Search, MessageCircle, Trash2, LogOut, Edit3, Bot, FileText } from "lucide-react";
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
      <div className="sidebar-header">
        <div className="sidebar-header-left">
          <div className="sidebar-avatar">{userInitial}</div>
        </div>
        <div className="sidebar-header-icons">
          <button className="btn-icon" onClick={() => setShowDocuments(true)} title="Dokümanlar">
            <FileText size={20} />
          </button>
          <button className="btn-icon" onClick={handleNew} title="Yeni Sohbet">
            <Edit3 size={20} />
          </button>
          <button className="btn-icon" onClick={() => authStore.logout()} title="Çıkış Yap">
            <LogOut size={20} />
          </button>
        </div>
      </div>

      {showDocuments && <DocumentsModal onClose={() => setShowDocuments(false)} />}

      <div className="sidebar-search">
        <div className="sidebar-search-input-wrap">
          <Search size={16} />
          <input
            className="sidebar-search-input"
            placeholder="Sohbet ara..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </div>

      <nav className="sidebar-nav" style={{ position: "relative" }}>
        {filtered.length === 0 && (
          <div style={{ padding: "24px 16px", textAlign: "center", color: "#8696a0", fontSize: 13 }}>
            {search ? "Sohbet bulunamadı" : "Henüz sohbet yok"}
          </div>
        )}
        {filtered.map((c) => (
          <div
            key={c.id}
            className={`sidebar-item ${activeId === c.id ? "active" : ""}`}
            onClick={() => onSelect(c.id, c.title)}
          >
            <div className="conv-avatar">
              <Bot size={24} />
            </div>
            <div className="conv-info">
              <div className="conv-info-top">
                <span className="conv-name">{c.title || "Yeni Sohbet"}</span>
                <span className="conv-time">{formatTime(c.updatedAt)}</span>
              </div>
              <div className="conv-preview">Sohbete devam et...</div>
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

        <button className="sidebar-new-btn" onClick={handleNew} title="Yeni Sohbet">
          <MessageCircle size={24} />
        </button>
      </nav>
    </aside>
  );
}
