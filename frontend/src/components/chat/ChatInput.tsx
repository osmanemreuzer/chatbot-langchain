import { useState, type KeyboardEvent } from "react";
import { Send, Smile } from "lucide-react";

interface Props {
  onSend: (content: string) => void;
  disabled: boolean;
}

export default function ChatInput({ onSend, disabled }: Props) {
  const [value, setValue] = useState("");

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

  return (
    <div className="chat-input-area">
      <div className="chat-input-box">
        <button className="btn-emoji" title="Emoji">
          <Smile size={22} />
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
