import { useRef, useState, type ChangeEvent, type KeyboardEvent } from "react";
import { Paperclip, Send } from "lucide-react";

interface Props {
  onSend: (content: string) => void;
  disabled: boolean;
}

export default function ChatInput({ onSend, disabled }: Props) {
  const [value, setValue] = useState("");
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleSend = () => {
    const trimmed = value.trim();
    if (!trimmed || disabled) return;
    onSend(trimmed);
    setValue("");
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // TODO: step 19'da doküman yükleme backend'ine bağlanacak
      console.log("Dosya seçildi:", file.name);
    }
    e.target.value = "";
  };

  return (
    <div className="chat-input-area">
      <div className="chat-input-box">
        <input
          ref={fileInputRef}
          type="file"
          hidden
          onChange={handleFileChange}
        />
        <button
          className="btn-emoji"
          title="Dosya Ekle"
          onClick={() => fileInputRef.current?.click()}
          disabled={disabled}
        >
          <Paperclip size={22} />
        </button>
        <textarea
          className="chat-input"
          value={value}
          onChange={(e) => setValue(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Mesaj yaz..."
          rows={1}
          disabled={disabled}
        />
      </div>
      <button
        className="btn-send"
        onClick={handleSend}
        disabled={disabled || !value.trim()}
        title="Gönder"
      >
        <Send size={20} />
      </button>
    </div>
  );
}
