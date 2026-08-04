import ReactMarkdown from "react-markdown";
import { Sparkles } from "lucide-react";
import type { Message } from "../../types";

interface Props {
  message: Message;
}

export default function MessageBubble({ message }: Props) {
  const isUser = message.role === "user";
  const time = new Date(message.createdAt).toLocaleTimeString("tr-TR", {
    hour: "2-digit",
    minute: "2-digit",
  });

  if (isUser) {
    return (
      <div className="message-row user">
        <div className="message-bubble user">
          <p>{message.content}</p>
          <div className="message-meta">
            <span className="message-time">{time}</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="message-row assistant">
      <div className="message-avatar">
        <Sparkles size={14} />
      </div>
      <div className="message-content">
        <div className="message-text">
          <ReactMarkdown>{message.content}</ReactMarkdown>
        </div>
        <div className="message-meta">
          <span className="message-time">{time}</span>
        </div>
      </div>
    </div>
  );
}
