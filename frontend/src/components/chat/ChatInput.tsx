import { useEffect, useRef, useState, type ChangeEvent, type KeyboardEvent } from "react";
import { Paperclip, Send } from "lucide-react";

interface Props {
  onSend: (content: string) => void;
  disabled: boolean;
}

export default function ChatInput({ onSend, disabled }: Props) {
  const [value, setValue] = useState("");
  const fileInputRef = useRef<HTMLInputElement>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // disabled (AI cevap beklerken) true->false olunca textarea'yı otomatik
  // odakla; browser disabled input'tan focus'u kaybediyor ve geri vermiyor.
  useEffect(() => {
    if (!disabled) {
      textareaRef.current?.focus();
    }
  }, [disabled]);

  // textarea rows={1} ile sabit tek satır yükseklikte; yazı uzayınca kutu
  // büyümediği için fazla kelimeler görünmez oluyordu. İçerik değiştikçe
  // yüksekliği scrollHeight'a göre yeniden hesaplıyoruz (CSS'teki max-height
  // sınırına kadar büyür, sonrası kutu içinde kayar).
  useEffect(() => {
    const el = textareaRef.current;
    if (!el) return;
    el.style.height = "auto";
    el.style.height = `${el.scrollHeight}px`;
  }, [value]);

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
          ref={textareaRef}
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
