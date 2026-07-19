import { useEffect, useRef, useState } from "react";
import { X, FileText, Trash2, Upload } from "lucide-react";
import { getDocuments, uploadDocument, deleteDocument } from "../../api/documents";
import type { DocumentItem } from "../../types";

interface Props {
  onClose: () => void;
}

function formatSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export default function DocumentsModal({ onClose }: Props) {
  const [documents, setDocuments] = useState<DocumentItem[]>([]);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState("");
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    getDocuments().then((r) => setDocuments(r.data));
  }, []);

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = "";
    if (!file) return;

    setUploading(true);
    setError("");
    try {
      const { data } = await uploadDocument(file);
      setDocuments((prev) => [data, ...prev]);
    } catch (err: any) {
      setError(err.response?.data?.error || "Yükleme başarısız oldu.");
    } finally {
      setUploading(false);
    }
  };

  const handleDelete = async (id: string) => {
    await deleteDocument(id);
    setDocuments((prev) => prev.filter((d) => d.id !== id));
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Dokümanlar</h2>
          <button className="btn-icon" onClick={onClose} title="Kapat">
            <X size={20} />
          </button>
        </div>

        <div className="modal-body">
          <button
            className="btn-primary"
            onClick={() => fileInputRef.current?.click()}
            disabled={uploading}
          >
            <Upload size={16} style={{ marginRight: 8, verticalAlign: "middle" }} />
            {uploading ? "Yükleniyor..." : "Doküman Yükle"}
          </button>
          <input
            ref={fileInputRef}
            type="file"
            accept=".pdf,.txt,.docx,.md"
            style={{ display: "none" }}
            onChange={handleFileChange}
          />

          {error && <div className="error-msg">{error}</div>}

          <div className="document-list">
            {documents.length === 0 && (
              <div style={{ padding: "24px 0", textAlign: "center", color: "var(--text-muted)", fontSize: 13 }}>
                Henüz doküman yüklenmedi
              </div>
            )}
            {documents.map((d) => (
              <div key={d.id} className="document-item">
                <FileText size={20} />
                <div className="document-info">
                  <span className="document-name">{d.fileName}</span>
                  <span className="document-meta">{formatSize(d.sizeBytes)}</span>
                </div>
                <button
                  className="conv-delete"
                  style={{ opacity: 1 }}
                  onClick={() => handleDelete(d.id)}
                  title="Sil"
                >
                  <Trash2 size={15} />
                </button>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
