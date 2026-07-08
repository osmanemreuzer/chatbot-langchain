import ReactMarkdown from "react-markdown";
import { CheckCheck } from "lucide-react";
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

  return (
    <div className={`message-row ${isUser ? "user" : "assistant"}`}>
      <div className={`message-bubble ${isUser ? "user" : "assistant"}`}>
        {isUser ? (
          <p>{message.content}</p>
        ) : (
          <ReactMarkdown>{message.content}</ReactMarkdown>
        )}
        <div className="message-meta">
          <span className="message-time">{time}</span>
          {isUser && <CheckCheck size={14} style={{ color: "#53bdeb" }} />}
        </div>
      </div>
    </div>
  );
}
