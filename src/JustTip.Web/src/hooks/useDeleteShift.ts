import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../lib/api'

async function deleteShift(id: number): Promise<void> {
  await api.delete(`/roster/shifts/${id}`)
}

export function useDeleteShift() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: deleteShift,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roster'] })
    },
  })
}
