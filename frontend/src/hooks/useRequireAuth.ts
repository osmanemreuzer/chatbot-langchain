import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { authStore } from "../store/authStore";

export const useRequireAuth = () => {
  const navigate = useNavigate();

  useEffect(() => {
    if (!authStore.isAuthenticated()) {
      navigate("/login");
    }
  }, [navigate]);
};
